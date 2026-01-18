using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;

namespace DiplomObsAlarm;

public static class GridPresets
{
    public static RowDefinitionCollection AutoRows(int count)
    {
        var cols = new RowDefinitionCollection();
        for (int i = 0; i < count; i++)
            cols.Add(new RowDefinition { Height = GridLength.Auto });
        return cols;
    } 
    public static ColumnDefinitionCollection AutoCols(int count)
    {
        var cols = new ColumnDefinitionCollection();
        for (int i = 0; i < count; i++)
            cols.Add(new ColumnDefinition { Width = GridLength.Auto });
        return cols;
    }//чёаниче
    public static RowDefinitionCollection AutoRow => AutoRows(19);
    public static ColumnDefinitionCollection AutoCol => AutoCols(40);
}