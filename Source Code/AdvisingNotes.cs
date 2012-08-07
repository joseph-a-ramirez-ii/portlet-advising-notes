using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jenzabar.Portal.Framework;
using Jenzabar.Portal.Framework.Web;
using Jenzabar.Portal.Framework.Web.UI;
using Jenzabar.Portal.Framework.Web.Common;
using Jenzabar.Portal.Framework.Configuration;
using Jenzabar.Framework.Authentication;
using Jenzabar.Portal.Framework.Security.Authorization;
using Jenzabar.Common.Web.UI.Controls;

namespace AdvisingNotes
{
    [PortletOperation("CanAdminPortlet",
        "Can administer portlet",
        "Whether the user can fully administer the portlet.",
        PortletOperationScope.Portlet)]
    [PortletOperation("ViewAll", 
        "Can view all advisee notes", 
        "Whether the user can see all notes for all advisees.",
        PortletOperationScope.Global)]
    [PortletOperation("EditAll", 
        "Can view and edit all advisee notes", 
        "Whether the user can edit all notes for all advisees.",
        PortletOperationScope.Global)]
 
    public class AdvisingNotes: SecuredPortletBase
    {
        protected override PortletViewBase GetCurrentScreen()
        {
            PortletViewBase screen = null;
            switch (this.CurrentPortletScreenName)
            {
                case "Main": screen = this.LoadPortletView("ICS/AdvisingNotes/Default_View.ascx");
                    break;
                default: screen = this.LoadPortletView("ICS/AdvisingNotes/Default_View.ascx");
                    break;
            }
            return screen;
        }
    }
}
