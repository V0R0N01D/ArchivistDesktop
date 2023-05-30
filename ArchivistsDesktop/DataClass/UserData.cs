using Avalonia.Controls;
using ArchivistsDesktop.View;
using System.Collections.Generic;

namespace ArchivistsDesktop.DataClass
{
    internal static class UserData
    {
        private static Stack<UserControl> previousPages = new Stack<UserControl>();

        internal static UserControl? GetPreviousPage()
        {
            return previousPages.TryPop(out UserControl? previousPage) ? previousPage : null;
        }

        internal static void PutPreviousPage(UserControl previousPage)
        {
            previousPages.Push(previousPage);
        }

        internal static DefaultWindow? currentWindow;

        internal static UserControl? currentPage;
    }
}
