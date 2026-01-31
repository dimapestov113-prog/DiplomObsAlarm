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
        //grid.SizeChanged += (s, e) => Recalc(grid); //кароче он работает если меняем размер
        //Recalc(grid);//эта работает если просто запустили прогу в первый раз))))
    }

    //private static void Recalc(Grid grid)
    //{
    //   //подсчет ячейки ширина и высота делятся на кол во нужных ячеек

    //    grid.RowDefinitions.Clear();//тут кароче можно было бы просто создавать через число и звездочку, но кароче так пизже
    //    for (int i = 0; i < 40; i++)
    //        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.1, GridUnitType.Star) });

    //    grid.ColumnDefinitions.Clear();
    //    for (int i = 0; i < 19; i++)
    //        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.1, GridUnitType.Star) });
    //}
}
//    private static void OnGridLoaded(object sender, EventArgs e)
//    {
//        //кароче тут я подписался на событие создания страницы и указал количестко столбцов и строк
//        //все теперь заебись работает и создается че надо и где надо, все поделено на колоночки и ахуенно бля все!!!GOOD BRUH
//        // AI SOSI BLYA DOLBAEB КАРОЧЕ ИИ НЕ ДОДУМАЛСЯ СДЕЛАТЬ ТАК, ПОШЕЛ НАХУЙ ХЫПАВХПАХВПХХВАПХХХПХЫХВАХЫХВХЫХЫХХЫХЫХЫХЫХЫХ))))

//        GeneralSetting.RowDefinitions.Clear();
//        for (int i = 0; i < 40; i++)
//            GeneralSetting.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
//        GeneralSetting.ColumnDefinitions.Clear();
//        for (int i = 0; i < 19; i++)
//            GeneralSetting.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.1, GridUnitType.Star) });

//        GeneralSetting.Loaded -= OnGridLoaded;
//    }
//    //чёаниче
//}