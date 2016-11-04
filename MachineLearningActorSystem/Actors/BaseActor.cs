namespace MachineLearningActorSystem.Actors
{
    using Akka.Actor;
    using Events;
    using log4net;

    public abstract class BaseActor : ReceiveActor
    {
        protected readonly ILog logger; 

        public BaseActor()
        {
            logger = LogManager.GetLogger(GetType());

            Receive<ShutdownEvent>(shutdownEvent =>
            {
                logger.InfoFormat("{0} Received", shutdownEvent.GetType().Name);
                Context.Stop(Self);
            });
        }

        protected override void PreStart()
        {
            logger.InfoFormat("Started\n- ActorPath: {0}", Self.Path.ToString());
            base.PreStart();
        }

        protected override void PostStop()
        {
            logger.InfoFormat("Stopped\n- ActorPath: {0}", Self.Path.ToString());
            base.PostStop();
        }
    }
}
