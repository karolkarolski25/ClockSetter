namespace SystemClockSetterNTP.Services
{
    public interface IWindowService
    {
        void SetWindowDimensions(int newWidth, int newHeight);
        void WindowServiceStartup();
        void SetWindowTitle(string newTitle);
    }
}
