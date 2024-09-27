using RevitAddinTest.Models;
using RevitAddinTest.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitAddinTest.Views
{
	/// <summary>
	/// Interaction logic for RoomDataExtractionView.xaml
	/// </summary>
	public partial class RoomDataExtractionView : Window
	{
		public RoomDataExtractionView()
		{
			InitializeComponent();
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            RoomModel selectedRoom = dataGrid.SelectedItem as RoomModel;

            if (selectedRoom != null)
            {
                RoomDataExtractionViewModel viewModel = this.DataContext as RoomDataExtractionViewModel;
                if (viewModel != null && viewModel.ZoomToRoomCommand.CanExecute(selectedRoom))
                {
                    viewModel.ZoomToRoomCommand.Execute(selectedRoom);
                }
            }

        }
    }
}
