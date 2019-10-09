using C1.WPF.FlexGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace SillyMonkey
{
    public class MyCellFactory : CellFactory
    {
        public override void CreateCellContentEditor(C1FlexGrid grid, Border bdr, CellRange rng)
        {
            if (grid.Columns[rng.Column].ColumnName == "LastOrderDate" && !grid.Columns[rng.Column].Format.Contains("t"))
            {
                DatePicker date = new DatePicker();
                Binding binding = new Binding("LastOrderDate");
                binding.Mode = BindingMode.TwoWay;
                date.SetBinding(DatePicker.SelectedDateProperty, binding);
                bdr.Child = date;
            }
            else
            {
                base.CreateCellContentEditor(grid, bdr, rng);
            }

        }
    }
}
