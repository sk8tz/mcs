//
// System.Web.UI.IPostBackDataHandler.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;

namespace System.Web.UI
{
        public interface IPostBackDataHandler
        {
                bool LoadPostData(string postDataKey, NameValueCollection postCollection);
                void RaisePostDataChangedEvent();
        }
}
