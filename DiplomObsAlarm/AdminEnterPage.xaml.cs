namespace DiplomObsAlarm;

public partial class AdminEnterPage : ContentPage
{
	public AdminEnterPage()
    {
        InitializeComponent();
        GeneralSetting.RowDefinitions = GridPresets.AutoRow;
        GeneralSetting.ColumnDefinitions = GridPresets.AutoCol;
    }
}