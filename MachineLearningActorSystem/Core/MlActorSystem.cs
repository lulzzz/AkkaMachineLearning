namespace MachineLearningActorSystem.Core
{
    using Actors.Classifiers;
    using Actors.Explorers;
    using Akka.Actor;
    using Events;
    using log4net;
    using System;

    public sealed class MlActorSystem
    {
        private static ActorSystem ClusterSystem;
        private static IActorRef _classifierCoordinatorActor;
        private static IActorRef _explorerCoordinatorActor;
        public static IActorRef ClassifierCoordinatorActor
        {
            get
            {
                if (Config.CoreEnableClassifierActors && _classifierCoordinatorActor == null)
                {
                    _classifierCoordinatorActor = ClusterSystem.ActorOf(Props.Create<ClassifierCoordinatorActor>(), typeof(ClassifierCoordinatorActor).Name);
                }

                return _classifierCoordinatorActor;
            }
        }

        public static IActorRef ExplorerCoordinatorActor
        {
            get
            {
                if (Config.CoreEnableExplorerActors && _explorerCoordinatorActor == null)
                {
                    _explorerCoordinatorActor = ClusterSystem.ActorOf(Props.Create<ExplorerCoordinatorActor>(), typeof(ExplorerCoordinatorActor).Name);
                }

                return _explorerCoordinatorActor;
            }
        }

        private static void RegisterDefaultAkkaCluster()
        {
            ClusterSystem = ActorSystem.Create(Constants.MachineLearningSystem);
        }

        private static void UnRegisterDefaultAkkaCluster()
        {
            var shutdownEvent = new ShutdownEvent();

            if (Config.CoreEnableClassifierActors)
            {
                ClassifierCoordinatorActor.GracefulStop(TimeSpan.FromSeconds(shutdownEvent.GetTimeoutSeconds()), shutdownEvent).Wait();
            }

            if (Config.CoreEnableExplorerActors)
            {
                ExplorerCoordinatorActor.GracefulStop(TimeSpan.FromSeconds(shutdownEvent.GetTimeoutSeconds()), shutdownEvent).Wait();
            }

            ClusterSystem.Terminate().Wait();
        }

        public static void Start()
        {
            var logger = LogManager.GetLogger(typeof(MlActorSystem));

            string akkaClusterError = string.Empty;

            try
            {
                RegisterDefaultAkkaCluster();
            }
            catch (Exception ex)
            {
                akkaClusterError = "Error registering Akka Cluster";
                akkaClusterError = string.Format("[{0}:{1}]", akkaClusterError, ex.ToString());
            }

            if (!string.IsNullOrEmpty(akkaClusterError))
            {
                var errorMessage = string.Format("Error starting Application: {0}", akkaClusterError);
                logger.Error(errorMessage);

                throw new Exception(errorMessage);
            }
        }

        public static void Stop()
        {
            var logger = LogManager.GetLogger(typeof(MlActorSystem));

            string akkaClusterError = string.Empty;

            try
            {
                UnRegisterDefaultAkkaCluster();
            }
            catch (Exception ex)
            {
                akkaClusterError = "Error unregistering Akka Cluster";
                akkaClusterError = $"[{akkaClusterError}:{ex.ToString()}]";
            }

            if (!string.IsNullOrEmpty(akkaClusterError))
            {
                var errorMessage = $"Error stopping Application: {akkaClusterError}";
                logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public static void Reaper()
        {
            var logger = LogManager.GetLogger(typeof(MlActorSystem));

            string akkaClusterError = string.Empty;

            try
            {
                if (Config.CoreEnableClassifierActors)
                {
                    ClassifierCoordinatorActor.Tell(Kill.Instance);
                }

                if (Config.CoreEnableExplorerActors)
                {
                    ExplorerCoordinatorActor.Tell(Kill.Instance);
                }

                ClusterSystem.Terminate();
            }
            catch (Exception ex)
            {
                akkaClusterError = "Akka Cluster Killed";
                akkaClusterError = $"[{akkaClusterError}:{ex}]";
            }

            if (!string.IsNullOrEmpty(akkaClusterError))
            {
                var errorMessage = $"Application death: {akkaClusterError}";
                logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
