using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;

namespace DiplomObsAlarm;

public static class GridPresets
{
    public static void Razmetka(this Grid grid, int a)
    {
        grid.RowDefinitions.Clear();//тут кароче можно было бы просто создавать через число и звездочку, но кароче так пизже
        for (int i = 0; i < a; i++)
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.1, GridUnitType.Star) });

        grid.ColumnDefinitions.Clear();
        for (int i = 0; i < 19; i++)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.1, GridUnitType.Star) });
    }

   
}