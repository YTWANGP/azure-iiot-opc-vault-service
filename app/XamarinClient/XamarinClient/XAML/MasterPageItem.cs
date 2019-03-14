using System;

namespace XamarinClient.XAML
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class MasterPageItem
	{
		public string Title { get; set; }

		public string IconSource { get; set; }

		public Type TargetType { get; set; }
	}
}
