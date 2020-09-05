using System;
using SystemClockSetterNTP.Models;
using Microsoft.Extensions.Logging;

namespace SystemClockSetterNTP.Services
{
    public class WindowService : IWindowService
    {
        private readonly ILogger<WindowService> _logger;
        private readonly WindowConfiguration _windowConfiguration;

        public WindowService(ILogger<WindowService> logger, WindowConfiguration windowConfiguration)
        {
            _logger = logger;
            _windowConfiguration = windowConfiguration;
        }

        private int GetOldWindowHeight()
        {
            return Console.WindowHeight;
        }

        private int GetOldWindowWidth()
        {
            return Console.WindowWidth;
        }

        public void WindowServiceStartup()
        {
            _logger.LogDebug($"Changing window dimensions, old dimensions: width: {GetOldWindowWidth()}, height: {GetOldWindowHeight()}");

            SetWindowDimensions(_windowConfiguration.Width, _windowConfiguration.Height);
            SetWindowTitle(_windowConfiguration.Title);
        }

        public void SetWindowDimensions(int newWidth, int newHeight)
        {
            Console.SetWindowSize(newWidth, newHeight);

            _logger.LogDebug($"Set new window dimensions (width: {newWidth} | height: {newHeight})");
        }

        public void SetWindowTitle(string newTitle)
        {
            _logger.LogDebug($"Setting new window title to: {newTitle}");

            Console.Title = newTitle;
        }
    }
}
