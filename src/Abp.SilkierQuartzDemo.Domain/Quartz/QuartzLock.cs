namespace Abp.SilkierQuartzDemo.Quartz
{
  public class QuartzLock
  {
    public string SchedulerName { get; set; } = null!;
    public string LockName { get; set; } = null!;
  }
}
