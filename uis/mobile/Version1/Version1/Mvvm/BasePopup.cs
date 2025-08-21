using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm;

public class BasePopup : BasePage
{
    public BasePopup()
    {
        Shell.SetPresentationMode(this, PresentationMode.Modal);
    }
}
