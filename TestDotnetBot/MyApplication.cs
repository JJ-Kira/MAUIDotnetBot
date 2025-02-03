using Evergine.Framework;
using Evergine.Framework.Managers;
using Evergine.Framework.Services;
using Evergine.Framework.Threading;
using Evergine.Platform;
using System;
using System.Linq;
using TestDotnetBot.Behaviors;

namespace TestDotnetBot
{
    public partial class MyApplication : Application
    {
        private CameraBehavior cameraBehavior;

        public MyApplication()
        {
            this.Container.Register<Settings>();
            this.Container.Register<Clock>();
            this.Container.Register<TimerFactory>();
            this.Container.Register<Evergine.Framework.Services.Random>();
            this.Container.Register<ErrorHandler>();
            this.Container.Register<ScreenContextManager>();
            this.Container.Register<GraphicsPresenter>();
            this.Container.Register<AssetsDirectory>();
            this.Container.Register<AssetsService>();
            this.Container.Register<ForegroundTaskSchedulerService>();
            this.Container.Register<WorkActionScheduler>();
        }

        public override void Initialize()
        {
            base.Initialize();

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();
            var assetsService = this.Container.Resolve<AssetsService>();

            // Navigate to scene
            var scene = assetsService.Load<MyScene>(EvergineContent.Scenes.MyScene_wescene);
            ScreenContext screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);

            // Ensure we fetch the CameraBehavior correctly from a known entity path
            this.cameraBehavior = scene.Managers.EntityManager.FindAllByTag("rotation").First().FindComponent<CameraBehavior>();
        }

        public void RotateLeft()
        {
            this.cameraBehavior?.RotateLeft();
        }

        public void RotateRight()
        {
            this.cameraBehavior?.RotateRight();
        }
    }
}


