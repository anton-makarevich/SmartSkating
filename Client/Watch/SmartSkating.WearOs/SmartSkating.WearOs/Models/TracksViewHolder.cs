using Android.Views;
using Android.Widget;
using Sanet.SmartSkating.ViewModels.Wrappers;

namespace Sanet.SmartSkating.WearOs.Models
{
    public class TracksViewHolder: ListViewHolder<TrackViewModel>
    {
        private TextView? Name { get; set; }

        public TracksViewHolder (ViewGroup parent) : base (LayoutInflater.From (parent.Context).
            Inflate (Resource.Layout.cell_track, parent, false))
        {
            Name = ItemView.FindViewById<TextView> (Resource.Id.textView);
        }

        public override void BindViewModel(TrackViewModel viewModel)
        {
            Name.Text = viewModel.Name;
        }
    }
}