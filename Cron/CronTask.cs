namespace Azure_PV111.Cron
{
    public class CronTask
    {
        private static List<CronAction> actions = new();
        private static bool isActive;
        public static void Add(Action action, int seconds)
        {
            actions.Add(new()
            {
                Action = action,
                Milliseconds = seconds * 1000
            });
        }
        public static void Start()
        {
            isActive = true;
            actions.ForEach(Execute);
        }
        public static void Stop()
        {
            isActive = false;
        }
        private static async void Execute(CronAction cronAction)
        {
            if (isActive)
            {
                //System.Console.WriteLine("CronTask " + action.Milliseconds);
                cronAction.Action.Invoke();
                await Task.Delay(cronAction.Milliseconds);
                Execute(cronAction);
            }            
        }
    }

    class CronAction
    {
        public Action Action { get; set; } = null!;
        public int Milliseconds { get; set; }
    }
}
