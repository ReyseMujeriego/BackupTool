using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySave.Core.Models;

namespace EasySave.Core.Services
{
    /// <summary>
    /// Interface that defines a save operation for jobs.
    /// It provides a contract for different save strategies (Complete or Differential).
    /// </summary>
    public interface ISave
    {
        /// <summary>
        /// Event triggered when a message is logged during a save operation.
        /// This can be used for UI updates.
        /// </summary>
        event Action<string> OnMessageLogged;

        /// <summary>
        /// Executes a save operation based on the provided job.
        /// The implementation will differ depending on whether the save is complete or differential.
        /// </summary>
        /// <param name="job">The job to be executed.</param>
        public void ExecuteSave(Job job);
        
    }
}