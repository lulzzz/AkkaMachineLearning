using Akka.Actor;
using log4net;
using MachineLearningActorSystem.Events;

namespace MachineLearningActorSystem.Actors
{
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
            logger.InfoFormat("Started\n- ActorPath: {0}", Self.Path);
            base.PreStart();
        }

        protected override void PostStop()
        {
            logger.InfoFormat("Stopped\n- ActorPath: {0}", Self.Path);
            base.PostStop();
        }
    }
}