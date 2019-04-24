using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using XamarinClient.Configuration;

namespace XamarinClient.UWP.Configuration
{
    public class UwpConfigurationStreamProvider : IConfigurationStreamProvider
    {
        private IRandomAccessStreamWithContentType _inputStream;
        private Stream _readingStream;

        public async Task<Stream> GetStreamAsync()
        {
            ReleaseUnmanagedResources();
            StorageFile file;

            try {
                StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
                file = await LocalFolder.GetFileAsync("xamarin.appsettings.json");
            }
            catch { 
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                picker.FileTypeFilter.Add(".json");

                file = await picker.PickSingleFileAsync();

                await file.CopyAsync(ApplicationData.Current.LocalFolder, "xamarin.appsettings.json", NameCollisionOption.ReplaceExisting);
            }
            _inputStream = await file.OpenReadAsync();
            _readingStream = _inputStream.AsStreamForRead();
            
            return _readingStream;
        }

        private void ReleaseUnmanagedResources()
        {
            _inputStream?.Dispose();
            _readingStream?.Dispose();

            _inputStream = null;
            _readingStream = null;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~UwpConfigurationStreamProvider()
        {
            ReleaseUnmanagedResources();
        }
    }
}
