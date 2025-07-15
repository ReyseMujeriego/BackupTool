namespace EasySave.Core.Models
{
    public enum SaveTypes
    {
        Complete,
        Differential
    }

    public  class Job
    {
        public string Name { get; set; } = "";
        public string SourcePath { get; set; } = "";
        public string DestinationPath { get; set; } = "";
        public SaveTypes SaveType { get; set; }
      
    }
   

}
