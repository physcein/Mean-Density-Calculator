using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using System.Data;
//using System.Text;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;

[assembly: AssemblyVersion("1.0.0.1")]

namespace VMS.TPS
{
    public class Script
    {

        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context, Window window)
        {
            PlanSetup planSetup = context.PlanSetup;
            PlanSum psum = context.PlanSumsInScope.FirstOrDefault();
            if (planSetup == null && psum == null)
                return;

            window.Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
            window.Background = System.Windows.Media.Brushes.Cornsilk;
            window.Height = 90;
            window.Width = 540;

            SelectedPlanningItem = planSetup != null ? (PlanningItem)planSetup : (PlanningItem)psum;
            // Plans in plansum can have different structuresets but here we only use structureset to allow chosing one structure
            SelectedStructureSet = planSetup != null ? planSetup.StructureSet : psum.PlanSetups.First().StructureSet;

            window.Title = "Mass Density Calculator for " + SelectedPlanningItem.Id + " / " + SelectedStructureSet.Id;


            if (SelectedPlanningItem.Dose == null)
                return;

            InitializeUI(window);
        }

        void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            return;
        }

        void InitializeUI(Window window)
        {
            StackPanel rootPanel = new StackPanel();
            rootPanel.Orientation = Orientation.Horizontal;

            {
                GroupBox structureGroup = new GroupBox();
                structureGroup.Header = "Structure";
                rootPanel.Children.Add(structureGroup);

                StackPanel structurePanel = new StackPanel();
                structurePanel.Orientation = Orientation.Horizontal;

                ComboBox structureCombo = new ComboBox();
                structureCombo.ItemsSource = SelectedStructureSet.Structures;
                structureCombo.SelectionChanged += new SelectionChangedEventHandler(OnComboSelectionChanged);
                structureCombo.MinWidth = 180.0;

                Label volumeLabel = new Label();
                volumeLabel.Content = "";
                m_structureDensity.Text = "Mean HU = meanHU" + " \u00B1 " + "stdHU(%), Mean Density = ________";
                m_structureDensity.Height = 20.0;
                m_structureDensity.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                structureGroup.Content = structurePanel;

                structurePanel.Children.Add(structureCombo);
                structurePanel.Children.Add(volumeLabel);
                structurePanel.Children.Add(m_structureDensity);

            }
            // Layout
            {
                m_structureDensity.MinWidth = 60.0;
                window.Content = rootPanel;
            }

        }

        private void OnComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                Structure structure = e.AddedItems[0] as Structure;
                if (structure != null)
                {
                    SelectedStructure = structure;
                    UpdateDensityCalc();
                }
            }
        }

        void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            UpdateDensityCalc();
        }

        void OnInputChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDensityCalc();
        }

        private void OnPercentageTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDensityCalc();
        }
        void UpdateDensityCalc()
        {
            if (SelectedStructure == null)
                return;
                m_structureDensity.Text = "";
                m_structureDensity.Text = MeanDensityCalc(SelectedStructureSet.Image, SelectedStructure);
        }

        public string MeanDensityCalc(VMS.TPS.Common.Model.API.Image image, Structure st)
        {
            List<int> voxelHUs = GetInterior(image, st);
            double avgHU = voxelHUs.Average();

            // Introduce your own HU to Density table below using List

            List<double> HUList = new List<double>();
            List<double> DensityList = new List<double>();
                
            HUList.Add(-1500.0);DensityList.Add(0.000);
            HUList.Add(-1000.0); DensityList.Add(0.001);
            HUList.Add(-696.6); DensityList.Add(0.280);
            HUList.Add(-510.9); DensityList.Add(0.480);
            HUList.Add(-81.5); DensityList.Add(0.949);
            HUList.Add(-40.0); DensityList.Add(0.985);
            HUList.Add(6.7); DensityList.Add(1.000);
            HUList.Add(7.5); DensityList.Add(1.019);
            HUList.Add(15.5); DensityList.Add(1.019);
            HUList.Add(29.9); DensityList.Add(1.051);
            HUList.Add(82.5); DensityList.Add(1.093);
            HUList.Add(232.7); DensityList.Add(1.146);
            HUList.Add(234.4); DensityList.Add(1.152);
            HUList.Add(443.6); DensityList.Add(1.331);
            HUList.Add(815.4); DensityList.Add(1.558);
            HUList.Add(1233.7); DensityList.Add(1.822);
            HUList.Add(2297.7); DensityList.Add(2.710);
            HUList.Add(6000.0); DensityList.Add(4.300);
            HUList.Add(7000.0); DensityList.Add(8.100);
            HUList.Add(8000.0); DensityList.Add(10.800);
            HUList.Add(9000.0); DensityList.Add(19.290);
            HUList.Add(20000.0); DensityList.Add(19.290);
            
            double closestHU = HUList.OrderBy(HU => Math.Abs(avgHU - HU)).First();

            double AvgDensity = 0.0;

            int n1;
            int n2;

            if (avgHU > closestHU)
            {
                n1 = HUList.IndexOf(closestHU);
                n2 = HUList.IndexOf(closestHU) + 1;
            }
            else
            {
                n1 = HUList.IndexOf(closestHU) -1;
                n2 = HUList.IndexOf(closestHU);
            }
            double a = (DensityList[n1] - DensityList[n2]) / (HUList[n1] - HUList[n2]);
            double b = DensityList[n1] - a * HUList[n1];
            AvgDensity = a * avgHU + b;

            //Console.WriteLine("Mean:" + avgHU);
            //Put it all together      
            double stdHU = Math.Abs(Math.Sqrt((voxelHUs.Sum(HU => Math.Pow(HU - avgHU, 2))) / (voxelHUs.Count() - 1)) / avgHU * 100);
            //Console.WriteLine("STD:" + stdHU);

            string MeanDensity = "Mean HU = " + avgHU.ToString("F1") + " \u00B1 " + stdHU.ToString("F1") + "%, Mean Density = " + AvgDensity.ToString("F3");

            return MeanDensity;// AvgDensity;
        }

        PlanningItem SelectedPlanningItem { get; set; }
        StructureSet SelectedStructureSet { get; set; }
        Structure SelectedStructure { get; set; }

        TextBlock m_structureDensity = new TextBlock();

        static List<int> GetInterior(VMS.TPS.Common.Model.API.Image HUN, Structure st)
        {
            List<int> resultHU = new List<int>();
            VVector p = new VVector();
            Rect3D st_bounds = st.MeshGeometry.Bounds;

            int zlim = 2;
            int ylim = 10;
            int xlim = 10;

            if (/*(st_bounds.SizeZ / HUN.ZRes) <= 10 || */(st_bounds.SizeY / HUN.YRes) <= 15 || (st_bounds.SizeX / HUN.XRes) <= 15)
            {
                //zlim = 2;
                ylim = (int)((st_bounds.SizeY / HUN.YRes) / 2);
                xlim = (int)((st_bounds.SizeX / HUN.XRes) / 2);
            }

            for (int z = 0; z < HUN.ZSize; z+=zlim)
            {
                for (int y = 0; y < HUN.YSize; y+=ylim)
                {
                    for (int x = 0; x < HUN.XSize; x+=xlim)
                    {
                        p.x = x * HUN.XRes;
                        p.y = y * HUN.YRes;
                        p.z = z * HUN.ZRes;

                        p = p + HUN.Origin;

                        if (st_bounds.Contains(p.x, p.y, p.z) // trimming
                            && st.IsPointInsideSegment(p)) // this is an expensive call
                        {
                            int[,] voxelCTNs = new int[HUN.XSize, HUN.YSize];
                            HUN.GetVoxels(z, voxelCTNs);
                            resultHU.Add(voxelCTNs[x, y]-1000);
                        }
                    }
                }

                GC.Collect(); // do this to avoid time out
                GC.WaitForPendingFinalizers();
            }
            return resultHU;
        }
    }
}
