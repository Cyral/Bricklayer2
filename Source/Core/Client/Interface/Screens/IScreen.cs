using System;
using Bricklayer.Client.Interface;

namespace Bricklayer.Core.Client.Interface.Screens
{
    public interface IScreen
    {
        /// <summary>
        /// Action to be performed when the screen is initialized. (When the controls are added)
        /// </summary>
        Action Initialized { get; set; }
        void Add(ScreenManager screenManager);
        void Remove();
    }
}
