using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedActivityManager.Models.Decorators
{
    public interface IActivityExtra
    {
        string GetDescription();
        int GetExtraCost();  // în minute
        string GetIcon();
        Task ExecuteAsync(Activity activity);
        bool IsEnabled { get; }
        string Name { get; }
    }
}
