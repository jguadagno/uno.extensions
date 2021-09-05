﻿using System;

namespace Uno.Extensions.Navigation
{
    public interface INavigationService
    {
        NavigationResult Navigate(NavigationRequest request);

        INavigationService ParentNavigation();

        INavigationService ChildNavigation();
    }
}
