using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace Farand_Chart_Lib_Ver3
{
    public partial class Farand_Chart: UserControl
    {
        Bitmap chart_Image;
        Bitmap graphArea_Image;
        Bitmap legends_Image;
        Graphics gChart;
        Graphics gGraphArea;
        Graphics gLegends;

        double xScale = 1;
        double xOffset = 1;
        double yScale = 1;
        double yOffset = 1;

        int mouseWheel_Delta = 0;
        bool chart_Entered = false;
        bool zoom_Active = false;
        bool pan_Active = false;
        bool window_Zoom_Active = false;
        bool zoom_All_Active = false;
        bool view_Changed = false;
        bool zoom_Window_Visible = false;
        bool is_Graph_Centered = false;
        bool pan_Restarted = false;
        bool mouse_In_Graph_Area = false;
        double x_zoom_Scale = 1;
        double y_zoom_Scale = 1;
        double x_total_Zoom_Scale = 1;
        double y_total_Zoom_Scale = 1;
        double pictureBox_Scale = 1;

        PointF pan_XY_Start = new PointF(0, 0);
        PointF pan_XY_End = new PointF(0, 0);
        PointF pan_XY_Displacement = new PointF(0, 0);
        PointF zoom_XY_Center = new PointF(0, 0);

        PointF window_Zoom_XY_Start = new PointF(0, 0);
        PointF window_Zoom_XY_End = new PointF(0, 0);

        PointF mouse_Position = new PointF(0, 0);


        #region <Events>
        public event EventHandler View_Changed;
        #endregion

        #region <General Classes>
        public enum LineType
        {
            Solid,
            Dashed,
            Dotted
        }
     
        public enum PointType
        {
            Circle,
            Square
        }

        public enum NumberFormat
        {
            Integral,
            Scientific,
            FloatingPoint
        }
       
        public class LineStyle
        {

            LineType lineType = LineType.Solid;
            public LineType Type
            {
                get { return lineType; }
                set { lineType = value; }
            }

            Color lineColor = Color.Lime;
            public Color Color
            {
                get { return lineColor; }
                set { lineColor = value; }
            }

            float lineWidth = 1.0F;
            public float Width
            {
                get { return lineWidth; }
                set { lineWidth = value; }
            }

            bool lineVisible = true;
            public bool Visible
            {
                get { return lineVisible; }
                set { lineVisible = value; }
            }

            public LineStyle()
            {

            }
        }

        public class PointStyle
        {
            PointType pointType = PointType.Circle;
            public PointType Type
            {
                get { return pointType; }
                set { pointType = value; }
            }

            Color pointFillColor = Color.Red;
            public Color FillColor
            {
                get { return pointFillColor; }
                set { pointFillColor = value; }
            }

            Color pointLineColor = Color.Red;
            public Color LineColor
            {
                get { return pointLineColor; }
                set { pointLineColor = value; }
            }

            float pointLineWidth = 1.0F;
            public float LineWidth
            {
                get { return pointLineWidth; }
                set { pointLineWidth = value; }
            }

            float pointSize = 1.0F;
            public float Size
            {
                get { return pointSize; }
                set { pointSize = value; }
            }

            bool pointsVisible = true;
            public bool Visible
            {
                get { return pointsVisible; }
                set { pointsVisible = value; }
            }

            public PointStyle()
            {

            }
        }

        public class TitleStyle
        {
            bool title_Is_Visible = true;
            public bool Visible
            {
                get { return title_Is_Visible; }
                set { title_Is_Visible = value; }
            }

            LabelStyle title_LabelStyle = new LabelStyle();
            public LabelStyle LabelStyle
            {
                get { return title_LabelStyle; }
                set { title_LabelStyle = value; }
            }

            SizeF title_Frame_Size = new SizeF();
            public SizeF FrameSize
            {
                get { return title_Frame_Size; }
                set { title_Frame_Size = value; }
            }

            Color title_Frame_Fill_Color = Color.FromArgb(32, 32, 32);
            public Color FillColor
            {
                get { return title_Frame_Fill_Color; }
                set { title_Frame_Fill_Color = value; }
            }

            LineStyle title_Frame_Line_Style = new LineStyle();
            public LineStyle LineStyle
            {
                get { return title_Frame_Line_Style; }
                set { title_Frame_Line_Style = value; }
            }
       

            float title_Frame_Left_Margin = 10.0F;
            public float LeftMargin
            {
                get { return title_Frame_Left_Margin; }
                set { title_Frame_Left_Margin = value; }
            }

            float title_Frame_Top_Margin = 10.0F;
            public float TopMargin
            {
                get { return title_Frame_Top_Margin; }
                set { title_Frame_Top_Margin = value; }
            }

            float title_Frame_Right_Margin = 10.0F;
            public float RightMargin
            {
                get { return title_Frame_Right_Margin; }
                set { title_Frame_Right_Margin = value; }
            }

            float title_Frame_Bottom_Margin = 10.0F;
            public float BottomMargin
            {
                get { return title_Frame_Bottom_Margin; }
                set { title_Frame_Bottom_Margin = value; }
            }

            PointStyle title_Marker = new PointStyle();
            public PointStyle Marker
            {
                get { return title_Marker; }
                set { title_Marker = value; }
            }

            string title_Text = "";
            public string Text
            {
                get { return title_Text; }
                set { title_Text = value; }
            }

            public TitleStyle()
            {

            }
        }

        public class Point_Data
        {          

            PointF pointXY_Data = new PointF();
            public PointF XY_Data
            {
                get { return pointXY_Data; }
                set { pointXY_Data = value; }
            }

            LineStyle lineStyle = new LineStyle();
            public LineStyle LineStyle
            {
                get { return lineStyle; }
                set { lineStyle = value; }
            }

            PointStyle pointStyle = new PointStyle();
            public PointStyle PointStyle
            {
                get { return pointStyle; }
                set { pointStyle = value; }
            }

            public Point_Data()
            {

            }
        }

        public class Grid
        {
            LineStyle gridLine = new LineStyle();
            public LineStyle LineStyle
            {
                get { return gridLine; }
                set { gridLine = value; }
            }

            float gridInterval = 0;
            public float Interval
            {
                get { return gridInterval; }
                set { gridInterval = value; }
            }

            bool gridVisible = true;
            public bool Visible
            {
                get { return gridVisible; }
                set { gridVisible = value; }
            }

            LabelStyle gridLabels = new LabelStyle();
            public LabelStyle Labels
            {
                get { return gridLabels; }
                set { gridLabels = value; }
            }

            public Grid()
            {

            }
        }

        public class LabelStyle
        {
            Font labelFont = new Font("Arial", 6.0F, GraphicsUnit.Pixel);
            public Font Font
            {
                get { return labelFont; }
                set { labelFont = value; }
            }

            bool labelVisible = true;
            public bool Visible
            {
                get { return labelVisible; }
                set { labelVisible = value; }
            }

            float labelOffset = 10.0F;
            public float Offset
            {
                get { return labelOffset; }
                set { labelOffset = value; }
            }

            Color labelColor = Color.Black;
            public Color Color
            {
                get { return labelColor; }
                set { labelColor = value; }
            }

            NumberFormat numberFormat = NumberFormat.FloatingPoint;
            public NumberFormat NumberFormat
            {
                get { return numberFormat; }
                set { numberFormat = value; }
            }

            int decimalPlaces = 1;
            public int DecimalPlaces
            {
                get { return decimalPlaces; }
                set { decimalPlaces = value; }
            }

            public LabelStyle()
            {

            }
        }

        public class Frame
        {
            public event EventHandler Frame_Changed;
            Color fillColor = Color.Gray;
            public Color FillColor
            {
                get { return fillColor; }
                set { fillColor = value; }
            }

            LineStyle lineStyle = new LineStyle();
            public LineStyle LineStyle
            {
                get { return lineStyle; }
                set { lineStyle = value; }
            }

            float leftMargin = 50.0F;
            public float LeftMargin
            {
                get { return leftMargin; }
                set
                {
                    leftMargin = value;
                    if (Frame_Changed != null)
                    {
                        Frame_Changed(null, null);
                    }
                }
            }

            float rightMargin = 20.0F;
            public float RightMargin
            {
                get { return rightMargin; }
                set
                {
                    rightMargin = value;
                    if (Frame_Changed != null)
                    {
                        Frame_Changed(null, null);
                    }
                }
            }

            float topMargin = 20.0F;
            public float TopMargin
            {
                get { return topMargin; }
                set
                {
                    topMargin = value;
                    if (Frame_Changed != null)
                    {
                        Frame_Changed(null, null);
                    }
                }
            }

            float bottomMargin = 50.0F;
            public float BottomMargin
            {
                get { return bottomMargin; }
                set
                {
                    bottomMargin = value;
                    if (Frame_Changed != null)
                    {
                        Frame_Changed(null, null);
                    }
                }
            }

            RectangleF drawingRectangle = new RectangleF(0, 0, 1, 1);
            public RectangleF DrawingRectangle
            {
                get { return drawingRectangle; }
                set { drawingRectangle = value; }
            }

            public Frame()
            {

            }

        }

        public class Axis
        {
            double minimum = 0;
            public double Minimum
            {
                get { return minimum; }
                set { minimum = value; }

            }

            double initial_Minimum = 0;
            public double Initial_Minimum
            {
                get { return initial_Minimum; }
                set
                {
                    initial_Minimum = value;
                    minimum = value;
                }
            }

            double maximum = 0;
            public double Maximum
            {
                get { return maximum; }
                set { maximum = value; }
            }

            double initial_Maximum = 0;
            public double Initial_Maximum
            {
                get { return initial_Maximum; }
                set
                {
                    initial_Maximum = value;
                    maximum = value;
                }
            }
            Grid majorGrid = new Grid();
            public Grid MajorGrid
            {
                get { return majorGrid; }
                set { majorGrid = value; }
            }

            Grid minorGrid = new Grid();
            public Grid MinorGrid
            {
                get { return minorGrid; }
                set { minorGrid = value; }
            }

            TitleStyle title = new TitleStyle();
            public TitleStyle Title
            {
                get { return title; }
                set { title = value; }
            }

            public Axis()
            {

            }
        }

        public class Farand_Graph
        {
            string name = "No Name";
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            ArrayList points = new ArrayList();
            public ArrayList Points
            {
                get { return points; }
                set { points = value; }
            }

            LineStyle line_Style = new LineStyle();
            public LineStyle LineStyle
            {
                get { return line_Style; }
                set { line_Style = value; }
            }

            PointStyle point_Style = new PointStyle();
            public PointStyle PointStyle
            {
                get { return point_Style; }
                set { point_Style = value; }
            }           

            public void Add_Point(double x, double y)
            {
                Point_Data p = new Point_Data();
                p.XY_Data = new PointF((float)x, (float)y);
                p.LineStyle = line_Style;
                p.PointStyle = point_Style;
                points.Add(p);
            }

            public Farand_Graph()
            {

            }

        }

        #endregion

        #region <Chart Properties>

        Color chartBackColor = Color.FromArgb(100, 100, 100);
        public Color ChartBackColor
        {
            get { return chartBackColor; }
            set { chartBackColor = value; }
        }

        Frame graphArea = new Frame();
        public Frame GraphArea
        {
            get { return graphArea; }
            set
            {
                graphArea = value;
            }
        }

        LineStyle zoom_Window_Frame = new LineStyle();
        public LineStyle Zoom_Window_Frame
        {
            get { return zoom_Window_Frame; }
            set { zoom_Window_Frame = value; }
        }

        TitleStyle chart_Title = new TitleStyle();
        public TitleStyle Title
        {
            get { return chart_Title; }
            set { chart_Title = value; }
        }

        TitleStyle coordinates_Title = new TitleStyle();
        public TitleStyle Coordinates
        {
            get { return coordinates_Title; }
            set { coordinates_Title = value; }
        }

        TitleStyle legends = new TitleStyle();
         public TitleStyle Legends
        {
            get { return legends; }
            set { legends = value; }
        }
        Axis xAxis = new Axis();
        public Axis XAxis
        {
            get { return xAxis; }
            set { xAxis = value; }
        }

        Axis yAxis = new Axis();
        public Axis YAxis
        {
            get { return yAxis; }
            set { yAxis = value; }
        }
       
        ArrayList graphs_Collection = new ArrayList(); 
        // !!! This as a public property causes Non-Serializable Exception !!!
        //public ArrayList Graphs_Collection
        //{
        //    get { return graphs_Collection; }
        //    set 
        //    { 
        //        graphs_Collection = value;
        //        Update_Legends_Image();
        //    }
        //}
        #endregion
        public void Add_Farand_Graph(Farand_Graph graph)
        {
            graphs_Collection.Add(graph);
            Update_Legends_Image();
        }

        public void Clear_All_Farand_Graphs()
        {
            graphs_Collection.Clear();           
        }

        public int Get_Farand_Graphs_Count()
        {
            return graphs_Collection.Count;
        }

        public Farand_Graph Get_Farand_Graph_Object(int index)
        {
            if (index>=0 && index < graphs_Collection.Count)
            {
                return (Farand_Graph)graphs_Collection[index] ;
            }
            else
            {
                return null;
            }
        }

        public void Refresh_All()
        {
            Update_Chart_Image();
            Update_GraphArea_Image();
            Update_Legends_Image();
        }

        public Farand_Chart()
        {
            this.Width = 100;
            this.Height = 100;
            InitializeComponent();
            Initialize_Chart();

            textBox1.Hide();
            Update_Chart_Image();
            Update_GraphArea_Image();
            Update_Legends_Image();
            pictureBox_Chart.MouseMove += pictureBox_Chart_MouseMove;
            pictureBox_Chart.MouseLeave += pictureBox_Chart_MouseLeave;
            pictureBox_Chart.MouseDown += pictureBox_Chart_MouseDown;
            pictureBox_Chart.MouseUp += pictureBox_Chart_MouseUp;
            pictureBox_Chart.MouseEnter += pictureBox_Chart_MouseEnter;
            pictureBox_Chart.MouseWheel += pictureBox_Chart_MouseWheel;
            pictureBox_Chart.Click += pictureBox_Chart_Click;
            pictureBox_Chart.Focus();

            graphArea.Frame_Changed += graphArea_Frame_Changed;
            Update_All();
        }

        void pictureBox_Chart_Click(object sender, EventArgs e)
        {
            
        }

        void graphArea_Frame_Changed(object sender, EventArgs e)
        {
            
        }       

        void pictureBox_Chart_MouseWheel(object sender, MouseEventArgs e)
        {
            mouseWheel_Delta = e.Delta;
        }

        void pictureBox_Chart_MouseEnter(object sender, EventArgs e)
        {
            pictureBox_Chart.Focus();
        }

        void pictureBox_Chart_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle:
                    //
                    break;
                case MouseButtons.Left:

                    //Check if mouse is in graph area                   
                    if (Is_Mouse_Within_GraphArea(e.X, e.Y) == true)
                    {
                        window_Zoom_XY_End = new PointF(mouse_Position.X, mouse_Position.Y);
                        window_Zoom_Active = true;
                        view_Changed = true;
                        zoom_Window_Visible = false;
                    }
                    if(Is_Mouse_Within_LegendsArea(e.X, e.Y) == true)
                    {                       
                        Update_Graph_Visibility(e.X, e.Y);
                    }
                    if (Is_Mouse_Within_SaveButton(e.X, e.Y) == true)
                    {
                        Save_Chart_Image();
                    }
                    break;
                case MouseButtons.Right:
                    if (Is_Mouse_Within_GraphArea(e.X, e.Y) == true)
                    {
                        zoom_All_Active = true;
                        view_Changed = true;
                    }
                    break;
            }
        }

        private void Save_Chart_Image()
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageCodecInfo myImageCodecInfo;
                System.Drawing.Imaging.Encoder myEncoder;
                
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
               

                // Get an ImageCodecInfo object that represents the JPEG codec.
                myImageCodecInfo = GetEncoderInfo("image/jpeg");

                // Create an Encoder object based on the GUID
                // for the Quality parameter category.
                myEncoder = System.Drawing.Imaging.Encoder.Quality;

                // Create an EncoderParameters object.
                // An EncoderParameters object has an array of EncoderParameter
                // objects. In this case, there is only one
                // EncoderParameter object in the array.
                myEncoderParameters = new EncoderParameters(1);
               
                // Save the bitmap as a JPEG file with quality level 75.
                myEncoderParameter = new EncoderParameter(myEncoder, 100L); 
                myEncoderParameters.Param[0] = myEncoderParameter;
                chart_Image.Save(saveFileDialog1.FileName, myImageCodecInfo, myEncoderParameters);               
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private bool Is_Mouse_Within_SaveButton(int xMouse, int yMouse)
        {
            float dH = legends.FrameSize.Height;
            float left = 0;
            float top = (float)(graphArea.TopMargin - 1.2 * dH);
            bool isSaveButton = false;

            if (legends.Visible == true)
            {
                left = this.Width - graphArea.RightMargin + legends.LeftMargin + legends_Image.Width - dH;
                if (xMouse > left && xMouse < left + dH && yMouse > top && yMouse < top + dH)
                {
                    isSaveButton = true;
                }
            }
            else
            {
                left = this.Width - graphArea.RightMargin + legends.LeftMargin;
                if (xMouse > left && xMouse < left + dH && yMouse > top && yMouse < top + dH)
                {
                    isSaveButton = true;
                }
            }        

            return isSaveButton;            
        }

        private void Update_Graph_Visibility(int xMouse, int yMouse)
        {
            float dH = legends.FrameSize.Height;
            float x = xMouse - (this.Width - graphArea.RightMargin + legends.LeftMargin);
            float y = yMouse - graphArea.TopMargin;
            float wLegend = legends_Image.Width;

            for (int k = 0; k < graphs_Collection.Count; k++)
            {
                Farand_Graph graph = (Farand_Graph)graphs_Collection[k];
                // Check Graph Visibilities
                if (x > 0 && x < 2 * dH && y > dH / 2 + k * dH && y < dH / 2 + (k + 1) * dH)
                {                    
                    graph.LineStyle.Visible = !graph.LineStyle.Visible;
                }

                // Check Point Visibilities
                if (x > wLegend - 1.8 * dH && x < wLegend - 0.5 * dH && y > dH / 2 + k * dH && y < dH / 2 + (k + 1) * dH)
                {
                    if (graph.LineStyle.Visible == true) // dont change point visibility for graphs which are not visible
                    {
                        graph.PointStyle.Visible = !graph.PointStyle.Visible;
                    }
                }

            }


            if (legends.Visible == true)
            {
                float wMax = legends.FrameSize.Width;
                float wMin = wMax - dH;
                float hMin = 0;
                float hMax = hMin + dH;

                if (x > wMin && x < wMax && y > hMin && y < hMax)
                {
                    legends.Visible = false;
                    graphArea.RightMargin = graphArea.RightMargin - ( legends.FrameSize.Width - legends.FrameSize.Height); // A square area is left for Legends Icon
                    Update_GraphArea_Image();
                    Update_Legends_Image();
                }            
            }
            else
            {
                float wMax = dH;
                float wMin = 0 ;
                float hMin = 0;
                float hMax = dH;

                if (x > wMin && x < wMax && y > hMin && y < hMax)
                {
                    legends.Visible = true;
                    graphArea.RightMargin = graphArea.RightMargin + (legends.FrameSize.Width - legends.FrameSize.Height); // A square area is left for Legends Icon
                    Update_GraphArea_Image();
                    Update_Legends_Image();
                }
            }
            //graph.LineStyle.Visible = !graph.LineStyle.Visible;

        }

        private bool Is_Mouse_Within_GraphArea(float x, float y)
        {
            bool withinArea = false;
            if (
                graphArea.LeftMargin < x
                &&
                x < this.Width - graphArea.RightMargin
                &&
                graphArea.TopMargin < y
                &&
                y < this.Height - graphArea.BottomMargin
               )
            {
                withinArea = true;
            }

            return withinArea;
        }

        private bool Is_Mouse_Within_LegendsArea(float x, float y)
        {
            bool withinArea = false;
            if (
                this.Width - graphArea.RightMargin + Legends.LeftMargin < x
                &&
                x < this.Width - graphArea.RightMargin + Legends.LeftMargin + legends_Image.Width
                &&
                graphArea.TopMargin < y
                &&
                y < graphArea.TopMargin + legends_Image.Height
               )
            {
                withinArea = true;
            }

            return withinArea;
        }
        void pictureBox_Chart_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle:
                    pan_XY_Start = new PointF(e.X, e.Y);
                    break;
                case MouseButtons.Left:
                    if (Is_Mouse_Within_GraphArea(e.X, e.Y) == true)
                    {
                        window_Zoom_XY_Start = new PointF(mouse_Position.X, mouse_Position.Y);
                        window_Zoom_XY_End = new PointF(mouse_Position.X, mouse_Position.Y);
                        zoom_Window_Visible = true;
                    }
                    break;
            }
        }

        void pictureBox_Chart_MouseLeave(object sender, EventArgs e)
        {
            chart_Entered = false;
            zoom_Active = false;
        }

        void pictureBox_Chart_MouseMove(object sender, MouseEventArgs e)
        {
            mouse_In_Graph_Area = Is_Mouse_Within_GraphArea(e.X, e.Y);

            // This function returns mouse position in pixels with respect to grap Area top left corner
            int left = (int)graphArea.LeftMargin;
            int top = (int)graphArea.TopMargin;

            int x = e.X - left;
            if (x < 0)
            {
                x = 0;
            }
            if (x > graphArea.DrawingRectangle.Width)
            {
                x = (int)graphArea.DrawingRectangle.Width;
            }

            int y = e.Y - top;
            if (y < 0)
            {
                y = 0;
            }
            if (y > graphArea.DrawingRectangle.Height)
            {
                y = (int)graphArea.DrawingRectangle.Height;
            }
            mouse_Position = new PointF(x, y);

            if (zoom_Window_Visible == true)
            {
                window_Zoom_XY_End = new PointF(mouse_Position.X, mouse_Position.Y);
            }

            pan_Active = false;
            switch (e.Button)
            {
                case MouseButtons.Middle: // Panning
                    if (Is_Mouse_Within_GraphArea(e.X, e.Y) == true)
                    {
                        pan_Active = true;
                        view_Changed = true;
                    }
                    break;
                case MouseButtons.Left: // Selecting Zoom Area                   
                    break;
            }
            chart_Entered = true;           
        }
       
        private void Initialize_Chart()
        {
            //Set Default Values for first run

            // Chart Settings
            chartBackColor = Color.FromArgb(100, 100, 100);


            // x Axis Settings
            xAxis.Initial_Minimum = -20;
            xAxis.Initial_Maximum = 100;

            xAxis.MajorGrid.Visible = true;
            xAxis.MajorGrid.Interval = 10;
            xAxis.MajorGrid.LineStyle.Width = 1.0F;
            xAxis.MajorGrid.LineStyle.Color = Color.FromArgb(120, 120, 120);
            xAxis.MajorGrid.Labels.Visible = true;
            xAxis.MajorGrid.Labels.Font = new Font("Arial", 8.0F);
            xAxis.MajorGrid.Labels.Color = Color.White;
            xAxis.MajorGrid.Labels.Offset = 15.0F;
            xAxis.MajorGrid.Labels.NumberFormat = NumberFormat.FloatingPoint;
            xAxis.MajorGrid.Labels.DecimalPlaces = 1;

            xAxis.MinorGrid.Visible = true;
            xAxis.MinorGrid.Interval = 2;
            xAxis.MinorGrid.LineStyle.Width = 1.0F;
            xAxis.MinorGrid.LineStyle.Color = Color.FromArgb(100, xAxis.MajorGrid.LineStyle.Color);
            xAxis.MinorGrid.Labels.Visible = true;
            xAxis.MinorGrid.Labels.Font = new Font("Arial", 8.0F);
            xAxis.MinorGrid.Labels.Color = Color.White;
            xAxis.MinorGrid.Labels.Offset = 15.0F;
            xAxis.MinorGrid.Labels.NumberFormat = NumberFormat.Scientific;
            xAxis.MinorGrid.Labels.DecimalPlaces = 0;

            // X Axis Title
            xAxis.Title.Text = "Time (uSec.)";
            xAxis.Title.Visible = true;
            xAxis.Title.FillColor = Color.FromArgb(30, 255, 255, 255);
            xAxis.Title.FrameSize = new SizeF(100.0F, 20.0F);
            xAxis.Title.TopMargin = -22.0F;
            xAxis.Title.LineStyle.Visible = false;
            xAxis.Title.LineStyle.Type = LineType.Solid;
            xAxis.Title.LineStyle.Color = Color.Black;
            xAxis.Title.LineStyle.Width = 1.0F;           
            xAxis.Title.LabelStyle.Color = Color.FromArgb(180, 255, 255, 255);
            xAxis.Title.LabelStyle.Font = new Font("Arial", 8.0F);
            xAxis.Title.LabelStyle.Offset = 1.0F;
            xAxis.Title.LabelStyle.Visible = true;
            xAxis.Title.LabelStyle.NumberFormat = NumberFormat.FloatingPoint;
            xAxis.Title.LabelStyle.DecimalPlaces = 1;
            xAxis.Title.Marker.Visible = false;


            // Y Axis Settings
            yAxis.Initial_Minimum = -75;
            yAxis.Initial_Maximum = 125;

            yAxis.MajorGrid.Visible = true;
            yAxis.MajorGrid.Interval = 25;
            yAxis.MajorGrid.LineStyle.Width = 0.5F;
            yAxis.MajorGrid.LineStyle.Color = Color.FromArgb(120, 120, 120);
            yAxis.MajorGrid.Labels.Visible = true;
            yAxis.MajorGrid.Labels.Font = new Font("Arial", 6.0F);
            yAxis.MajorGrid.Labels.Color = Color.LightGray;
            yAxis.MajorGrid.Labels.Offset = 20.0F;
            yAxis.MajorGrid.Labels.NumberFormat = NumberFormat.FloatingPoint;
            yAxis.MajorGrid.Labels.DecimalPlaces = 1;


            yAxis.MinorGrid.Visible = true;
            yAxis.MinorGrid.Interval = 5;
            yAxis.MinorGrid.LineStyle.Width = 0.5F;
            yAxis.MinorGrid.LineStyle.Color = Color.FromArgb(100, yAxis.MajorGrid.LineStyle.Color);
            yAxis.MinorGrid.Labels.Visible = true;
            yAxis.MinorGrid.Labels.Font = new Font("Arial", 6.0F);
            yAxis.MinorGrid.Labels.Color = Color.LightGray;
            yAxis.MinorGrid.Labels.Offset = 20.0F;
            yAxis.MinorGrid.Labels.NumberFormat = NumberFormat.FloatingPoint;
            yAxis.MinorGrid.Labels.DecimalPlaces = 1;

            // Y Axis Title
            yAxis.Title.Text = "Intensity (%)";
            yAxis.Title.Visible = true;
            yAxis.Title.FillColor = Color.FromArgb(30, 255, 255, 255);
            yAxis.Title.FrameSize = new SizeF(100.0F, 20.0F);
            yAxis.Title.RightMargin = -100.0F;
            yAxis.Title.LineStyle.Visible = false;
            yAxis.Title.LineStyle.Type = LineType.Solid;
            yAxis.Title.LineStyle.Color = Color.Black;
            yAxis.Title.LineStyle.Width = 1.0F;
            yAxis.Title.LabelStyle.Color = Color.FromArgb(180, 255, 255, 255);
            yAxis.Title.LabelStyle.Font = new Font("Arial", 8.0F);
            yAxis.Title.LabelStyle.Offset = 1.0F;
            yAxis.Title.LabelStyle.Visible = true;
            yAxis.Title.LabelStyle.NumberFormat = NumberFormat.FloatingPoint;
            yAxis.Title.LabelStyle.DecimalPlaces = 1;
            yAxis.Title.Marker.Visible = false;           

            legends.Visible = true;
            legends.FillColor = Color.FromArgb(64, 64, 64);
            legends.LineStyle.Visible = true;
            legends.LineStyle.Width = 1.5F;
            legends.LineStyle.Color = Color.FromArgb(180, 180, 180);
            legends.FrameSize = new SizeF(150.0F, 18.0F);
            legends.LeftMargin = 10.0F;
            legends.LabelStyle.Font = new Font("Arial", 8.0F);
            legends.LabelStyle.Color = Color.White;

            // Graph Area Settings
            graphArea.LeftMargin = 40.0F;
            graphArea.RightMargin = (int)(legends.FrameSize.Width + 2 * legends.LeftMargin);
            graphArea.TopMargin = 40.0F;
            graphArea.BottomMargin = 30.0F;

            graphArea.FillColor = Color.FromArgb(64, 64, 64);

            graphArea.LineStyle.Width = 2.5F;
            graphArea.LineStyle.Color = Color.LightGray;
            graphArea.LineStyle.Visible = true;

            zoom_Window_Frame.Visible = true;
            zoom_Window_Frame.Type = LineType.Solid;
            zoom_Window_Frame.Width = 1.0F;
            zoom_Window_Frame.Color = Color.Gold;

            // Fill Sample Data for graph1
            Farand_Graph graph1 = new Farand_Graph();
            graph1.Name = "Function No.1";
            graph1.LineStyle.Color = Color.Orange;
            graph1.LineStyle.Type = LineType.Solid;
            graph1.LineStyle.Visible = false;
            graph1.LineStyle.Width = 1.0F;
            graph1.PointStyle.FillColor = Color.Orange;
            graph1.PointStyle.LineColor = Color.Gold;
            graph1.PointStyle.LineWidth = 2.0F;
            graph1.PointStyle.Size = 4.0F;
            graph1.PointStyle.Type = PointType.Circle;
            graph1.PointStyle.Visible = true;
            for (int k = 0; k < 200; k++)
            {
                double x = k;
                double y = 100 * Math.Sin(3 * 3.14 * (k - 150) / 200);
                graph1.Add_Point(x, y);
            }
            Add_Farand_Graph(graph1);

            // Fill Sample Data for graph2
            Farand_Graph graph2 = new Farand_Graph();
            graph2.Name = "Function No.2";
            graph2.LineStyle.Color = Color.Tomato;
            graph2.LineStyle.Type = LineType.Solid;
            graph2.LineStyle.Visible = true;
            graph2.LineStyle.Width = 1.0F;
            graph2.PointStyle.FillColor = Color.Tomato;
            graph2.PointStyle.LineColor = Color.Tomato;
            graph2.PointStyle.LineWidth = 2.0F;
            graph2.PointStyle.Size = 4.0F;
            graph2.PointStyle.Type = PointType.Square;
            graph2.PointStyle.Visible = true;
            for (int k = 0; k < 200; k++)
            {
                double x = k;
                double y = 100 * Math.Sin(5 * 3.14 * (k - 50) / 200);
                graph2.Add_Point(x, y);
            }
            Add_Farand_Graph(graph2);

            // Fill Sample Data for graph3
            Farand_Graph graph3 = new Farand_Graph();
            graph3.Name = "Function No.3";
            graph3.LineStyle.Color = Color.LimeGreen;
            graph3.LineStyle.Type = LineType.Solid;
            graph3.LineStyle.Visible = true;
            graph3.LineStyle.Width = 1.0F;
            graph3.PointStyle.FillColor = Color.LimeGreen;
            graph3.PointStyle.LineColor = Color.SeaGreen;
            graph3.PointStyle.LineWidth = 2.0F;
            graph3.PointStyle.Size = 8.0F;
            graph3.PointStyle.Type = PointType.Square;
            graph3.PointStyle.Visible = false;
            for (int k = 0; k < 200; k++)
            {
                double x = k;
                double y = 100 * Math.Sin(7 * 3.14 * (k - 100) / 200);
                graph3.Add_Point(x, y);
            }
            Add_Farand_Graph(graph3);

            // Titles 
            // Chart Title
            chart_Title.Text = "Farand Chart 2022";
            chart_Title.Visible = true;
            chart_Title.FillColor = Color.FromArgb(30, 255, 255, 255);
            chart_Title.FrameSize = new SizeF(150.0F, 33.0F);
            chart_Title.BottomMargin = 4.0F;
            chart_Title.LineStyle.Visible = false;
            chart_Title.LineStyle.Type = LineType.Solid;
            chart_Title.LineStyle.Color = Color.Black;
            chart_Title.LineStyle.Width = 1.0F;
            chart_Title.LabelStyle.Color = Color.White;
            chart_Title.LabelStyle.Font = new Font("Arial", 12.0F);
            chart_Title.LabelStyle.Offset = 1.0F;
            chart_Title.LabelStyle.Visible = true;
            chart_Title.LabelStyle.NumberFormat = NumberFormat.FloatingPoint;
            chart_Title.LabelStyle.DecimalPlaces = 1;
            chart_Title.Marker.Visible = false;

            // Coordinates
            coordinates_Title.Text = "";
            coordinates_Title.Visible = true;
            coordinates_Title.FillColor = Color.FromArgb(20, 255, 255, 255);
            coordinates_Title.FrameSize = new SizeF(150.0F, 33.0F);
            coordinates_Title.BottomMargin = 4.0F;
            coordinates_Title.LineStyle.Visible = false;
            coordinates_Title.LineStyle.Type = LineType.Solid;
            coordinates_Title.LineStyle.Color = Color.Black;
            coordinates_Title.LineStyle.Width = 1.0F;
            coordinates_Title.LabelStyle.Color = Color.White;
            coordinates_Title.LabelStyle.Font = new Font("Arial", 12.0F);
            coordinates_Title.LabelStyle.Offset = 1.0F;
            coordinates_Title.LabelStyle.Visible = true;
            coordinates_Title.LabelStyle.NumberFormat = NumberFormat.FloatingPoint;
            coordinates_Title.LabelStyle.DecimalPlaces = 2;
            coordinates_Title.Marker.Visible = false;
         
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Update_Zoom_Scale();
            Update_Pan_Zoom();
            Update_All();
            Show_Data();
            Fire_View_Changed_Event();
        }

        private void Fire_View_Changed_Event()
        {
            if (view_Changed == true)
            {
                view_Changed = false;
                if (View_Changed != null)
                {
                    View_Changed(this, null);
                }
            }
        }

        private void Show_Data()
        {
            string s = "Chart Entered: " + chart_Entered.ToString() + "\r\n" +
                       "Mouse Wheel Delta: " + mouseWheel_Delta.ToString() + "\r\n" +
                       "Mouse Position: " + mouse_Position.X.ToString("0.0") + ", " + mouse_Position.Y.ToString("0.0") + "\r\n\r\n" +

                       "Zoom Active: " + zoom_Active.ToString() + "\r\n" +
                       "Zoom Scales X,Y: " + x_zoom_Scale.ToString(".0.00") + y_zoom_Scale.ToString(".0.00") + "\r\n" +
                       "Total Zoom Scales X,Y: " + x_total_Zoom_Scale.ToString("0.00") + y_total_Zoom_Scale.ToString("0.00") + "\r\n" +
                       "Zoom XY Center: " + zoom_XY_Center.X.ToString("0.0") + ", " + zoom_XY_Center.Y.ToString("0.0") + "\r\n\r\n" +

                       "Pan Active: " + pan_Active.ToString() + "\r\n" +
                       "Pan Start: " + pan_XY_Start.X.ToString() + ", " + pan_XY_Start.Y.ToString() + "\r\n" +
                       "Pan End: " + pan_XY_End.X.ToString() + ", " + pan_XY_End.Y.ToString() + "\r\n\r\n" +

                       "Window Zoom Active: " + window_Zoom_Active.ToString() + "\r\n" +
                       "Window Zoom Start: " + window_Zoom_XY_Start.X.ToString() + ", " + window_Zoom_XY_Start.Y.ToString() + "\r\n" +
                       "Window Zoom End: " + window_Zoom_XY_End.X.ToString() + ", " + window_Zoom_XY_End.Y.ToString() + "\r\n\r\n" +

                       "X minimum, maximum: " + xAxis.Minimum.ToString("0.00") + ", " + xAxis.Maximum.ToString("0.00") + "\r\n" +
                       "Y minimum, maximum: " + yAxis.Minimum.ToString("0.00") + ", " + yAxis.Maximum.ToString("0.00") + "\r\n"
                       ;

            textBox1.Text = s;
        }

        private void Update_Pan_Zoom()
        {
            /////// Calculate Physical Mouse Position ///////////////////////

            // Calculate Mouse Position on Map Image
            float X_Image = (float)(mouse_Position.X * pictureBox_Scale);
            float Y_Image = (float)(mouse_Position.Y * pictureBox_Scale);

            // Calculate Physical Mouse Position (Pysical) on Tunnel Data
            float X_Phys = 0;
            float Y_Phys = 0;

            X_Phys = (float)((X_Image - xOffset) / xScale);
            Y_Phys = (float)((Y_Image - yOffset) / yScale);

            zoom_XY_Center = new PointF(X_Phys, Y_Phys);

            // PAN ///////////////////
            #region < Pan Algorithm >
            if (pan_Active == true)
            {

                // Update Pan Start Position
                if (pan_Restarted == true)
                {
                    pan_XY_Start = new PointF(mouse_Position.X, mouse_Position.Y);
                    pan_Restarted = false;
                }
                else
                {
                    pan_XY_Start = new PointF(pan_XY_End.X, pan_XY_End.Y);
                }
                // Update Pan End Position
                pan_XY_End = new PointF(mouse_Position.X, mouse_Position.Y);


                /////// Calculate Physical Pan displacement ///////////////////////
                // Calculate Pixel Displacement on Picture Box
                float dX = (float)(pan_XY_End.X - pan_XY_Start.X);
                float dY = (float)(pan_XY_End.Y - pan_XY_Start.Y);

                // Calculate Pixel displacement on Map                
                dX = (float)(dX * pictureBox_Scale);
                dY = (float)(dY * pictureBox_Scale);
                // Calculate Physical Displacement (m) on Tunnel Data
                float dX_Pyhs = (float)(dX / xScale);
                float dY_Phys = (float)(dY / yScale);
                pan_XY_Displacement = new PointF(dX_Pyhs, dY_Phys);

                // Determine Displayed Window Bounds
                if ((xAxis.Minimum - pan_XY_Displacement.X >= xAxis.Initial_Minimum) && (xAxis.Maximum - pan_XY_Displacement.X <= xAxis.Initial_Maximum))
                {
                    xAxis.Minimum -= pan_XY_Displacement.X;
                    xAxis.Maximum -= pan_XY_Displacement.X;
                }
                if ((yAxis.Minimum - pan_XY_Displacement.Y >= yAxis.Initial_Minimum) && (yAxis.Maximum - pan_XY_Displacement.Y <= yAxis.Initial_Maximum))
                {
                    yAxis.Minimum -= pan_XY_Displacement.Y;
                    yAxis.Maximum -= pan_XY_Displacement.Y;
                }
                //}
                //else
                //{

                //}
            }
            else
            {
                // If pan is not active
                if (is_Graph_Centered == true)
                {
                    double dX = xAxis.Maximum - xAxis.Minimum;
                    double dY = yAxis.Maximum - yAxis.Minimum;

                    //xAxis.Minimum = robot.X - dX / 2;
                    //xAxis.Maximum = robot.X + dX / 2;
                    //yAxis.Minimum = robot.Y - dY / 2;
                    //yAxis.Maximum = robot.Y + dY / 2;
                }
                else
                {
                    pan_XY_Start = new PointF(mouse_Position.X, mouse_Position.Y);
                    pan_XY_End = new PointF(mouse_Position.X, mouse_Position.Y);
                    pan_XY_Displacement = new PointF(0, 0);
                }
            }
            #endregion

            //// ZOOM ////////////////////////////
            #region < Zoom Algorithm >
            if (zoom_Active == true)
            {
                // Check bounds for Zoom-In ////////////
                if (x_zoom_Scale == 1.2) // x Zoom-In
                {
                    // Check if maximum zoom in is reached            
                    if (x_total_Zoom_Scale > Math.Pow(1.2, 25)) // Levels of zoom-in is limited to 25
                    {
                        x_total_Zoom_Scale = Math.Pow(1.2, 25);
                        x_zoom_Scale = 1;
                    }
                    else
                    {
                        x_total_Zoom_Scale *= 1.2;
                    }
                }

                if (y_zoom_Scale == 1.2) // y Zoom-In
                {
                    // Check if maximum zoom in is reached            
                    if (y_total_Zoom_Scale > Math.Pow(1.2, 25)) // Levels of zoom-in is limited to 25
                    {
                        y_total_Zoom_Scale = Math.Pow(1.2, 25);
                        y_zoom_Scale = 1;
                    }
                    else
                    {
                        y_total_Zoom_Scale *= 1.2;
                    }
                }

                // Check bounds for Zoom-Out ////////////




                if (x_zoom_Scale == 1.0 / 1.2) // x Zoom-Out
                {
                    // Check if maximum zoom-out is reached
                    if (
                       (xAxis.Maximum >= xAxis.Initial_Maximum)
                       &&
                       (xAxis.Minimum <= xAxis.Initial_Minimum)
                    )
                    {
                        x_total_Zoom_Scale = 1;
                        x_zoom_Scale = 1;
                    }
                    else
                    {
                        x_total_Zoom_Scale /= 1.2;
                    }
                }

                if (y_zoom_Scale == 1.0 / 1.2) // y Zoom-Out
                {
                    // Check if maximum zoom-out is reached
                    if (
                       (yAxis.Maximum >= yAxis.Initial_Maximum)
                       &&
                       (yAxis.Minimum <= yAxis.Initial_Minimum)
                    )
                    {
                        y_total_Zoom_Scale = 1;
                        y_zoom_Scale = 1;
                    }
                    else
                    {
                        y_total_Zoom_Scale /= 1.2;
                    }
                }


                // Determine Displayed Window Bounds
                if (xAxis.Maximum - xAxis.Minimum <= xAxis.Initial_Maximum - xAxis.Initial_Minimum)
                {
                    // if area is shorter than Original Size , zooming in both x & y is allowed
                    xAxis.Minimum = zoom_XY_Center.X + (xAxis.Minimum - zoom_XY_Center.X) / x_zoom_Scale;
                    xAxis.Maximum = zoom_XY_Center.X + (xAxis.Maximum - zoom_XY_Center.X) / x_zoom_Scale;
                    yAxis.Minimum = zoom_XY_Center.Y + (yAxis.Minimum - zoom_XY_Center.Y) / y_zoom_Scale;
                    yAxis.Maximum = zoom_XY_Center.Y + (yAxis.Maximum - zoom_XY_Center.Y) / y_zoom_Scale;
                }
                else
                {
                    xAxis.Minimum = xAxis.Initial_Minimum;
                    xAxis.Maximum = xAxis.Initial_Maximum;
                    yAxis.Minimum = yAxis.Initial_Minimum;
                    yAxis.Maximum = yAxis.Initial_Maximum;
                }
                // Zoom is done
                x_zoom_Scale = 1;
                y_zoom_Scale = 1;


            }

            if (window_Zoom_Active == true)
            {

                x_total_Zoom_Scale = (xAxis.Initial_Maximum - xAxis.Initial_Minimum) / (xAxis.Maximum - xAxis.Minimum);
                y_total_Zoom_Scale = (yAxis.Initial_Maximum - yAxis.Initial_Minimum) / (yAxis.Maximum - yAxis.Minimum);

                window_Zoom_Active = false;

                if (x_total_Zoom_Scale <= Math.Pow(1.2, 25))
                {
                    // x Start must be less than x End, check for make zoom not sensitive to drag direction 
                    double x_Start = window_Zoom_XY_Start.X;
                    double x_End = window_Zoom_XY_End.X;
                    if (x_End < x_Start)
                    {
                        x_Start = window_Zoom_XY_End.X;
                        x_End = window_Zoom_XY_Start.X;
                    }

                    if (x_End - x_Start > (xAxis.Initial_Maximum - xAxis.Initial_Minimum) / Math.Pow(1.2, 25))
                    {
                        xAxis.Minimum = (float)((x_Start - xOffset) / xScale);
                        xAxis.Maximum = (float)((x_End - xOffset) / xScale);
                    }
                }

                if (y_total_Zoom_Scale <= Math.Pow(1.2, 25))
                {
                    // y Start must be more than y End, check for make zoom not sensitive to drag direction 
                    double y_Start = window_Zoom_XY_Start.Y;
                    double y_End = window_Zoom_XY_End.Y;
                    if (y_End > y_Start)
                    {
                        y_Start = window_Zoom_XY_End.Y;
                        y_End = window_Zoom_XY_Start.Y;
                    }

                    if (y_Start - y_End > (yAxis.Initial_Maximum - yAxis.Initial_Minimum) / Math.Pow(1.2, 25))
                    {
                        yAxis.Minimum = (float)((y_Start - yOffset) / yScale);
                        yAxis.Maximum = (float)((y_End - yOffset) / yScale);
                    }
                }
            }

            // Zoom All
            if (zoom_All_Active == true)
            {
                zoom_All_Active = false;
                // Reset Zoom
                xAxis.Minimum = xAxis.Initial_Minimum;
                xAxis.Maximum = xAxis.Initial_Maximum;

                yAxis.Minimum = yAxis.Initial_Minimum;
                yAxis.Maximum = yAxis.Initial_Maximum;

                x_total_Zoom_Scale = 1;
                y_total_Zoom_Scale = 1;
            }
            #endregion
        }

        void Update_Zoom_Scale()
        {
            if (chart_Entered == true)
            {
                if (mouseWheel_Delta > 0)
                {
                    x_zoom_Scale = 1.2;
                    y_zoom_Scale = 1.2;
                    zoom_Active = true;
                    view_Changed = true;
                    mouseWheel_Delta = 0;
                }
                else if (mouseWheel_Delta < 0)
                {
                    x_zoom_Scale = 1.0 / 1.2;
                    y_zoom_Scale = 1.0 / 1.2;
                    zoom_Active = true;
                    view_Changed = true;
                    mouseWheel_Delta = 0;
                }
                else
                {
                    zoom_Active = false;
                }
            }
            else
            {
                zoom_Active = false;
            }
        }

        private void Update_Chart_Image()
        {
            //if (chart_Image != null)
            //{
            //    chart_Image.Dispose();
            //}
            try
            {
                chart_Image = new Bitmap(this.Width, this.Height);
            }
            catch
            {
                chart_Image = new Bitmap(10, 10);
            }
        }

         private void Update_Legends_Image()
         {
             if (legends.Visible == true)
             {
                 int w = (int)legends.FrameSize.Width;
                 int h = (int)((graphs_Collection.Count + 1) * legends.FrameSize.Height);
                 try
                 {
                     legends_Image = new Bitmap(w, h, PixelFormat.Format64bppArgb);
                 }
                 catch
                 {
                     legends_Image = new Bitmap(10, 10, PixelFormat.Format64bppArgb);
                 }
                
             }
             else
             {
                 int w = (int)legends.FrameSize.Height;
                 int h = w;
                 try
                 {
                     legends_Image = new Bitmap(w, h, PixelFormat.Format64bppArgb);
                 }
                 catch
                 {
                     legends_Image = new Bitmap(10, 10, PixelFormat.Format64bppArgb);
                 }
                 
             }
         }
        private void Update_GraphArea_Image()
        {
            //if (graphArea_Image != null)
            //{
            //    graphArea_Image.Dispose();
            //}

            int w = (int)(this.Width - graphArea.LeftMargin - graphArea.RightMargin);
            int h = (int)(this.Height - graphArea.TopMargin - graphArea.BottomMargin);

            try
            {
                graphArea_Image = new Bitmap(w, h);
            }
            catch
            {
                graphArea_Image = new Bitmap(10, 10);
            }
        }

        private void Update_All()
        {

            Update_Background();

            Update_GraphArea();

            Draw_Graphs();

            Draw_Chart_Frame();

            Draw_GraphArea_In_Chart();

            Draw_Labels();

            Draw_Chart_Title();

            Draw_Chart_Coordinates();

            Draw_Chart_XAxis_Title();

            Draw_Chart_YAxis_Title();

            Draw_Legends();

            Draw_Save_Button();

            pictureBox_Chart.Image = chart_Image;
        }

        private void Draw_Save_Button()
        {
            float dH = legends.FrameSize.Height;
            float left = 0;
            float top =(float)(graphArea.TopMargin - 1.2*dH);
           

            if(legends.Visible == true)
            {
                left = this.Width - graphArea.RightMargin + legends.LeftMargin + legends_Image.Width - dH;   
            }
            else
            {
                left = this.Width - graphArea.RightMargin + legends.LeftMargin ;   
            }
            RectangleF rSave = new RectangleF(left, top, dH, dH);
            SolidBrush frameBrush = new SolidBrush(legends.FillColor);
            Pen framePen = new Pen(legends.LineStyle.Color, 1.0F);
            gChart.FillRectangle(frameBrush, rSave);
            gChart.DrawRectangle(framePen, rSave.Left, rSave.Top, rSave.Width, rSave.Height);

            // Draw Maximize Icon
            float iSize = (float)(legends.FrameSize.Height * 0.8);
            float dSize = (legends.FrameSize.Height - iSize) / 2;
            RectangleF iconRect = new RectangleF(0, 0, Properties.Resources.Save_Icon.Width, Properties.Resources.Save_Icon.Height);
            RectangleF destRect = new RectangleF(rSave.Left + dSize, rSave.Top + dSize, iSize, iSize);
            gChart.DrawImage(Properties.Resources.Save_Icon, destRect, iconRect, GraphicsUnit.Pixel);
        }

        private void Draw_Legends()
        {
            gLegends = Graphics.FromImage(legends_Image);
            gLegends.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Rectangle rLegends = new Rectangle(0, 0, legends_Image.Width, legends_Image.Height);
            SolidBrush frameBrush = new SolidBrush(legends.FillColor);
            Pen framePen = new Pen(legends.LineStyle.Color, legends.LineStyle.Width);
            gLegends.FillRectangle(frameBrush, rLegends);

            if (legends.LineStyle.Visible == true)
            {
                gLegends.DrawRectangle(framePen, 0, 0, legends_Image.Width - framePen.Width / 2, legends_Image.Height - framePen.Width / 2);
            }

            SizeF fontSize = gLegends.MeasureString("SS", legends.LabelStyle.Font);

            if (legends.Visible == true)
            {
                for (int k = 0; k < graphs_Collection.Count; k++)
                {
                    Farand_Graph graph = (Farand_Graph)graphs_Collection[k];
                    string name = graph.Name;
                    Font nameFont = legends.LabelStyle.Font;
                    SolidBrush nameBrush = new SolidBrush(legends.LabelStyle.Color);
                    float nameLeft = 2 * fontSize.Height;
                    float nameTop = legends.FrameSize.Height - fontSize.Height / 2 + k * legends.FrameSize.Height;
                    gLegends.DrawString(name, nameFont, nameBrush, new PointF(nameLeft, nameTop));

                    float markerSize = (float)(fontSize.Height * 0.8);
                    SolidBrush markerBrush = new SolidBrush(graph.LineStyle.Color);
                    Pen markerPen = new Pen(graph.LineStyle.Color, 1);
                    RectangleF markerRect = new RectangleF(markerSize, nameTop + (fontSize.Height - markerSize) / 2, markerSize, markerSize);
                    if (graph.LineStyle.Visible == true)
                    {
                        gLegends.FillRectangle(markerBrush, markerRect);
                    }
                    else
                    {
                        gLegends.DrawRectangle(markerPen, markerRect.Left, markerRect.Top, markerRect.Width, markerRect.Height);
                    }


                    // Draw Points Selection Circle
                    SolidBrush pointBrush = new SolidBrush(graph.PointStyle.FillColor);
                    Pen pointPen = new Pen(graph.PointStyle.LineColor, 0.5F);
                    float pointWidth = (float)(0.3*legends.FrameSize.Height);
                    RectangleF pointRect = new RectangleF( (float)(legends_Image.Width - 1.5* legends.FrameSize.Height -pointWidth / 2), nameTop + fontSize.Height / 2 - pointWidth / 2, pointWidth, pointWidth);
                    if(graph.LineStyle.Visible == true && graph.PointStyle.Visible == true)
                    {
                        // draw filled circle
                        gLegends.FillEllipse(pointBrush, pointRect);
                    }
                    else 
                    {
                        // draw hollow circle
                        gLegends.DrawEllipse(pointPen, pointRect.Left, pointRect.Top, pointRect.Width, pointRect.Height);
                    }


                    // Draw Minimize Icon
                    float iSize = (float)(legends.FrameSize.Height * 0.8);
                    float dSize = (legends.FrameSize.Height - iSize) / 2;
                    RectangleF iconRect = new RectangleF(0, 0, Properties.Resources.Minimize_Icon.Width, Properties.Resources.Minimize_Icon.Height);
                    RectangleF destRect = new RectangleF(legends_Image.Width - legends.FrameSize.Height, dSize, iSize, iSize);
                    gLegends.DrawImage(Properties.Resources.Minimize_Icon, destRect, iconRect, GraphicsUnit.Pixel);
                }
            }
            else
            {
                // Draw Maximize Icon
                float iSize = (float)(legends.FrameSize.Height * 0.8);
                float dSize = (legends.FrameSize.Height - iSize) / 2;
                RectangleF iconRect = new RectangleF(0, 0, Properties.Resources.Maximize_Icon.Width, Properties.Resources.Maximize_Icon.Height);
                RectangleF destRect = new RectangleF(dSize, dSize, iSize, iSize);
                gLegends.DrawImage(Properties.Resources.Maximize_Icon, destRect, iconRect, GraphicsUnit.Pixel);
            }

            float legendLeft = (int)(this.Width - graphArea.RightMargin + Legends.LeftMargin);
            float legendTop = (int)graphArea.TopMargin;

            gChart.DrawImage(legends_Image, new PointF(legendLeft, legendTop));
        }

        private void Draw_Chart_XAxis_Title()
        {
            // Draw Title Frame

            if (xAxis.Title.Visible == true)
            {
                SolidBrush sbFrame = new SolidBrush(xAxis.Title.FillColor);
                Pen linePen = new Pen(xAxis.Title.LineStyle.Color, xAxis.Title.LineStyle.Width);

                float frameWidth = xAxis.Title.FrameSize.Width;
                float frameHeight = xAxis.Title.FrameSize.Height;

                // Draw Title Background
                float frameLeft = this.Width - graphArea.RightMargin - frameWidth;
                float frameTop = this.Height - graphArea.BottomMargin + xAxis.Title.TopMargin;
                RectangleF rFrame = new RectangleF(frameLeft, frameTop, frameWidth, frameHeight);
                gChart.FillRectangle(sbFrame, rFrame);
                gChart.FillRectangle(sbFrame, rFrame);

                // Draw Title Border
                if (xAxis.Title.LineStyle.Visible == true)
                {
                    gChart.DrawRectangle(linePen, rFrame.Left, rFrame.Top, rFrame.Width, rFrame.Height);
                }

                // Draw Title Text              
                string labelString = xAxis.Title.Text;
                Font labelFont = xAxis.Title.LabelStyle.Font;
                SolidBrush labelBrush = new SolidBrush(xAxis.Title.LabelStyle.Color);
                SizeF labelSize = gChart.MeasureString(labelString, labelFont);
                float labelLeft = rFrame.Left + rFrame.Width / 2 - labelSize.Width / 2;
                float labelTop = rFrame.Top + rFrame.Height / 2 - labelSize.Height / 2;
                gChart.DrawString(labelString, labelFont, labelBrush, new PointF(labelLeft, labelTop));
            }
        }

        private void Draw_Chart_YAxis_Title()
        {
            // Draw Title Frame

            if (yAxis.Title.Visible == true)
            {
                SolidBrush sbFrame = new SolidBrush(yAxis.Title.FillColor);
                Pen linePen = new Pen(yAxis.Title.LineStyle.Color, yAxis.Title.LineStyle.Width);

                float frameWidth = yAxis.Title.FrameSize.Width;
                float frameHeight = yAxis.Title.FrameSize.Height;

                // Draw Title Background
                float frameLeft = graphArea.LeftMargin - frameWidth - yAxis.Title.RightMargin;
                float frameTop = graphArea.TopMargin;
                RectangleF rFrame = new RectangleF(frameLeft, frameTop, frameWidth, frameHeight);
                gChart.FillRectangle(sbFrame, rFrame);
                gChart.FillRectangle(sbFrame, rFrame);

                // Draw Title Border
                if (yAxis.Title.LineStyle.Visible == true)
                {
                    gChart.DrawRectangle(linePen, rFrame.Left, rFrame.Top, rFrame.Width, rFrame.Height);
                }

                // Draw Title Text              
                string labelString = yAxis.Title.Text;
                Font labelFont = yAxis.Title.LabelStyle.Font;
                SolidBrush labelBrush = new SolidBrush(yAxis.Title.LabelStyle.Color);
                SizeF labelSize = gChart.MeasureString(labelString, labelFont);
                float labelLeft = rFrame.Left + rFrame.Width / 2 - labelSize.Width / 2;
                float labelTop = rFrame.Top + rFrame.Height / 2 - labelSize.Height / 2;
                gChart.DrawString(labelString, labelFont, labelBrush, new PointF(labelLeft, labelTop));
            }
        }
        private void Draw_Chart_Coordinates()
        {
            if (mouse_In_Graph_Area == true)
            {
                // Draw Title Frame            

                if (coordinates_Title.Visible == true)
                {
                    SolidBrush sbFrame = new SolidBrush(coordinates_Title.FillColor);
                    Pen linePen = new Pen(coordinates_Title.LineStyle.Color, coordinates_Title.LineStyle.Width);

                    float frameWidth = coordinates_Title.FrameSize.Width;
                    float frameHeight = coordinates_Title.FrameSize.Height;

                    // Draw Title Background
                    float frameLeft = this.Width - graphArea.RightMargin - frameWidth;
                    float frameTop = graphArea.TopMargin - frameHeight - coordinates_Title.BottomMargin;
                    RectangleF rFrame = new RectangleF(frameLeft, frameTop, frameWidth, frameHeight);
                    gChart.FillRectangle(sbFrame, rFrame);
                    gChart.FillRectangle(sbFrame, rFrame);

                    // Draw Title Border
                    if (coordinates_Title.LineStyle.Visible == true)
                    {
                        gChart.DrawRectangle(linePen, rFrame.Left, rFrame.Top, rFrame.Width, rFrame.Height);
                    }

                    // Draw Title Text
                    string sX = Get_Formated_string(zoom_XY_Center.X, coordinates_Title.LabelStyle.NumberFormat, coordinates_Title.LabelStyle.DecimalPlaces);
                    string sY = Get_Formated_string(zoom_XY_Center.Y, coordinates_Title.LabelStyle.NumberFormat, coordinates_Title.LabelStyle.DecimalPlaces);
                    string labelString = "( " + sX + " , " + sY + " )";
                    Font labelFont = coordinates_Title.LabelStyle.Font;
                    SolidBrush labelBrush = new SolidBrush(coordinates_Title.LabelStyle.Color);
                    SizeF labelSize = gChart.MeasureString(labelString, labelFont);
                    float labelLeft = rFrame.Left + rFrame.Width / 2 - labelSize.Width / 2;
                    float labelTop = rFrame.Top + rFrame.Height / 2 - labelSize.Height / 2;
                    gChart.DrawString(labelString, labelFont, labelBrush, new PointF(labelLeft, labelTop));

                }
            }
        }

        private void Draw_Chart_Title()
        {
            // Draw Title Frame

            if (chart_Title.Visible == true)
            {
                SolidBrush sbFrame = new SolidBrush(chart_Title.FillColor);
                Pen linePen = new Pen(chart_Title.LineStyle.Color, chart_Title.LineStyle.Width);

                float frameWidth = chart_Title.FrameSize.Width;
                float frameHeight = chart_Title.FrameSize.Height;

                // Draw Title Background
                float frameLeft = graphArea.LeftMargin + graphArea.DrawingRectangle.Width / 2 - frameWidth / 2;
                float frameTop = graphArea.TopMargin - frameHeight - chart_Title.BottomMargin;
                RectangleF rFrame = new RectangleF(frameLeft, frameTop, frameWidth, frameHeight);
                gChart.FillRectangle(sbFrame, rFrame);

                // Draw Title Border
                if (chart_Title.LineStyle.Visible == true)
                {
                    gChart.DrawRectangle(linePen, rFrame.Left, rFrame.Top, rFrame.Width, rFrame.Height);
                }

                // Draw Title Text
                string labelString = chart_Title.Text;
                Font labelFont = chart_Title.LabelStyle.Font;
                SolidBrush labelBrush = new SolidBrush(chart_Title.LabelStyle.Color);
                SizeF labelSize = gChart.MeasureString(labelString, labelFont);
                float labelLeft = rFrame.Left + rFrame.Width / 2 - labelSize.Width / 2;
                float labelTop = rFrame.Top + rFrame.Height / 2 - labelSize.Height / 2;
                gChart.DrawString(labelString, labelFont, labelBrush, new PointF(labelLeft, labelTop));

            }

        }

        private void Draw_Labels()
        {
            double x = 0;
            double y = 0;
            float xp = 0;
            float yp = 0;
            float left = graphArea.LeftMargin;
            float right = this.Width - graphArea.RightMargin;

            float top = graphArea.TopMargin;
            float bottom = this.Height - graphArea.BottomMargin;

            SolidBrush sb = new SolidBrush(xAxis.MajorGrid.Labels.Color);
            Font labelFont = xAxis.MajorGrid.Labels.Font;

            string s = "";
            // Draw Vertical Grid Labels ( X axis Labels)
            if (xAxis.MajorGrid.Labels.Visible == true)
            {
                yp = this.Height - graphArea.BottomMargin + xAxis.MajorGrid.Labels.Offset;
                int k = (int)(xAxis.Minimum / xAxis.MajorGrid.Interval);
                do
                {
                    x = k * xAxis.MajorGrid.Interval;
                    xp = (float)(x * xScale + xOffset) + left;
                    if (xp >= left && xp <= right)
                    {
                        // Round to required decimal Places (0,1,2,3,4)
                        s = Get_Formated_string(x, xAxis.MajorGrid.Labels.NumberFormat, xAxis.MajorGrid.Labels.DecimalPlaces);
                        SizeF size = gChart.MeasureString(s, labelFont);
                        gChart.DrawString(s, labelFont, sb, new PointF(xp - size.Width / 2, yp - size.Height / 2));
                    }
                    k++;
                } while (xp <= right);
            }

            s = "";
            // Draw Horizontal Grid Labels ( X axis Labels)
            if (yAxis.MajorGrid.Labels.Visible == true)
            {
                xp = graphArea.LeftMargin - yAxis.MajorGrid.Labels.Offset;
                int k = (int)(yAxis.Minimum / yAxis.MajorGrid.Interval);
                do
                {
                    y = k * yAxis.MajorGrid.Interval;
                    yp = (float)(y * yScale + yOffset) + top;
                    if (yp >= top && yp <= bottom)
                    {
                        // Round to required decimal Places (0,1,2,3,4)
                        s = Get_Formated_string(y, yAxis.MajorGrid.Labels.NumberFormat, yAxis.MajorGrid.Labels.DecimalPlaces);
                        SizeF size = gChart.MeasureString(s, labelFont);
                        gChart.DrawString(s, labelFont, sb, new PointF(xp - size.Width / 2, yp - size.Height / 2));
                    }
                    k++;
                } while (yp >= top);
            }
        }

        string Get_Formated_string(double x, NumberFormat nF, int decPlaces)
        {
            string s = "0";
            switch (nF)
            {
                case NumberFormat.Integral:
                    s = string.Format("{0:D}", x);
                    break;
                case NumberFormat.FloatingPoint:
                    double xR = Math.Round(x, decPlaces);
                    switch (decPlaces)
                    {
                        case 0:
                            s = string.Format("{0:F0}", x);
                            break;
                        case 1:
                            s = xR.ToString("0.0");
                            break;
                        case 2:
                            s = xR.ToString("0.00");
                            break;
                        case 3:
                            s = xR.ToString("0.000");
                            break;
                        case 4:
                            s = xR.ToString("0.0000");
                            break;
                    }
                    break;
                case NumberFormat.Scientific:
                    s = string.Format("{0:#.##E+0}", x);
                    break;
            }
            return s;
        }


        private void Draw_GraphArea_In_Chart()
        {
            float left = graphArea.LeftMargin;
            float top = graphArea.TopMargin;
            gChart.DrawImage(graphArea_Image, new PointF(left, top));
        }

        private void Draw_Chart_Frame()
        {
            // Draw frame line of the chart area ----------------------------------------------------------------------------------
            float frame_Thickness = graphArea.LineStyle.Width;
            if (graphArea.LineStyle.Visible == true)
            {
                Pen linePen = new Pen(graphArea.LineStyle.Color, frame_Thickness);
                float xF = 0;
                float yF = 0;
                float wF = graphArea.DrawingRectangle.Width - 1;
                float hF = graphArea.DrawingRectangle.Height - 1;
                gGraphArea.DrawRectangle(linePen, xF, yF, wF, hF);
            }

        }

        private void Draw_Graphs()
        {

            for (int n = 0; n < graphs_Collection.Count; n++)
            {
                Farand_Graph graph = (Farand_Graph)graphs_Collection[n];
                if (graph.LineStyle.Visible == true)
                {
                    for (int k = 1; k < graph.Points.Count; k++)
                    {
                        Point_Data p0 = (Point_Data)(graph.Points[k - 1]);
                        Point_Data p1 = (Point_Data)(graph.Points[k]);
                        Pen linePen = new Pen(p1.LineStyle.Color, p1.LineStyle.Width);

                        double x0 = p0.XY_Data.X * xScale + xOffset;
                        double y0 = p0.XY_Data.Y * yScale + yOffset;
                        double x1 = p1.XY_Data.X * xScale + xOffset;
                        double y1 = p1.XY_Data.Y * yScale + yOffset;

                        gGraphArea.DrawLine(linePen, (float)x0, (float)y0, (float)x1, (float)y1);


                    }

                    // Draw Points
                    for (int k = 0; k < graph.Points.Count; k++)
                    {

                        Point_Data p1 = (Point_Data)(graph.Points[k]);
                        Pen linePen = new Pen(p1.LineStyle.Color, p1.LineStyle.Width);

                        double x1 = p1.XY_Data.X * xScale + xOffset;
                        double y1 = p1.XY_Data.Y * yScale + yOffset;

                        SolidBrush pointBrush = new SolidBrush(p1.PointStyle.FillColor);
                        Pen pointPen = new Pen(p1.PointStyle.LineColor, p1.PointStyle.LineWidth);
                        float pointSize = p1.PointStyle.Size;
                        RectangleF pointRect = new RectangleF((float)(x1 - pointSize / 2), (float)(y1 - pointSize / 2), pointSize, pointSize);


                        if (/*p1.In_Range == true && */p1.PointStyle.Visible == true)
                        {
                            if (p1.PointStyle.Type == PointType.Circle)
                            {
                                gGraphArea.FillEllipse(pointBrush, pointRect);
                                gGraphArea.DrawEllipse(pointPen, pointRect.Left, pointRect.Top, pointSize, pointSize);
                            }

                            if (p1.PointStyle.Type == PointType.Square)
                            {
                                gGraphArea.FillRectangle(pointBrush, pointRect);
                                gGraphArea.DrawRectangle(pointPen, pointRect.Left, pointRect.Top, pointSize, pointSize);
                            }
                        }
                    }
                }
            }
        }

        private void Update_Background()
        {
            gChart = Graphics.FromImage(chart_Image);
            gChart.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            SolidBrush sb = new SolidBrush(chartBackColor);
            gChart.FillRectangle(sb, ClientRectangle);
        }
      

        private void pictureBox_Chart_SizeChanged(object sender, EventArgs e)
        {
            Update_Chart_Image();
            Update_GraphArea_Image();
            Update_Legends_Image();
            xAxis.Minimum = xAxis.Initial_Minimum;
            xAxis.Maximum = xAxis.Initial_Maximum;
            yAxis.Minimum = yAxis.Initial_Minimum;
            yAxis.Maximum = yAxis.Initial_Maximum;
            x_total_Zoom_Scale = 1;
            y_total_Zoom_Scale = 1;
        }

        private void Update_GraphArea()
        {

            // Draw background for chart area
            float left = graphArea.LeftMargin;
            float top = graphArea.TopMargin;
            float width = this.Width - (graphArea.RightMargin + graphArea.LeftMargin);
            float height = this.Height - (graphArea.TopMargin + graphArea.BottomMargin);
            GraphArea.DrawingRectangle = new RectangleF(0, 0, width, height);

            gGraphArea = Graphics.FromImage(graphArea_Image);
            gGraphArea.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // Update Scale and Offset
            xScale = width / (xAxis.Maximum - xAxis.Minimum); // xAxis.Minimum, xAxis.Maximum maps to 0, width
            xOffset = -xAxis.Minimum * xScale;

            yScale = (0 - height) / (yAxis.Maximum - yAxis.Minimum); // yAxis.Minimum, yAxis.Maximum maps to height, 0
            yOffset = -yAxis.Maximum * yScale;

            // Fill Background ----------------------------------------------------------------------------------
            SolidBrush sb = new SolidBrush(graphArea.FillColor);
            gGraphArea.FillRectangle(sb, graphArea.DrawingRectangle);


            // Draw Vertical Minor Grid ----------------------------------------------------------------------------------
            if (xAxis.MinorGrid.Visible == true && y_total_Zoom_Scale >= 4)
            {
                double x = 0;
                float xp = 0;
                float y1p = 0;
                float y2p = height;
                Pen penG = new Pen(xAxis.MinorGrid.LineStyle.Color, xAxis.MinorGrid.LineStyle.Width);
                xAxis.MinorGrid.Interval = xAxis.MajorGrid.Interval / 5;
                int k = (int)(xAxis.Minimum / xAxis.MinorGrid.Interval);
                do
                {
                    x = k * xAxis.MinorGrid.Interval;
                    xp = (float)(x * xScale + xOffset);
                    if (xp > 0)
                    {
                        gGraphArea.DrawLine(penG, xp, y1p, xp, y2p);
                    }
                    k++;
                } while (xp < width);
            }


            // Draw Horizontal Minor Grid ----------------------------------------------------------------------------------
            if (yAxis.MinorGrid.Visible == true && x_total_Zoom_Scale >= 4)
            {
                double y = 0;
                float yp = 0;
                float x1p = 0;
                float x2p = width;
                Pen penG = new Pen(yAxis.MinorGrid.LineStyle.Color, yAxis.MinorGrid.LineStyle.Width);
                int k = (int)(yAxis.Minimum / yAxis.MinorGrid.Interval);
                do
                {
                    y = k * yAxis.MinorGrid.Interval;
                    yp = (float)(y * yScale + yOffset);
                    if (yp < height && yp > 0)
                    {
                        gGraphArea.DrawLine(penG, x1p, yp, x2p, yp);
                    }
                    k++;
                } while (yp > 0);
            }

            // Draw Vertical Grid ----------------------------------------------------------------------------------
            if (xAxis.MajorGrid.Visible == true)
            {
                double x = 0;
                float xp = 0;
                float y1p = 0;
                float y2p = height;
                Pen penG = new Pen(xAxis.MajorGrid.LineStyle.Color, xAxis.MajorGrid.LineStyle.Width);
                int k = (int)(xAxis.Minimum / xAxis.MajorGrid.Interval);
                do
                {
                    x = k * xAxis.MajorGrid.Interval;
                    xp = (float)(x * xScale + xOffset);
                    if (xp > 0)
                    {
                        gGraphArea.DrawLine(penG, xp, y1p, xp, y2p);
                    }
                    k++;
                } while (xp < width);
            }

            // Draw Horizontal Grid ----------------------------------------------------------------------------------
            if (yAxis.MajorGrid.Visible == true)
            {
                double y = 0;
                float yp = 0;
                float x1p = 0;
                float x2p = width;
                Pen penG = new Pen(yAxis.MajorGrid.LineStyle.Color, xAxis.MajorGrid.LineStyle.Width);
                int k = (int)(yAxis.Minimum / yAxis.MajorGrid.Interval);
                do
                {
                    y = k * yAxis.MajorGrid.Interval;
                    yp = (float)(y * yScale + yOffset);
                    if (yp < height && yp > 0)
                    {
                        gGraphArea.DrawLine(penG, x1p, yp, x2p, yp);
                    }
                    k++;
                } while (yp > 0);
            }

            // Draw zoom Window
            if (zoom_Window_Visible == true)
            {
                float wLeft;
                float wWidth;
                if (window_Zoom_XY_Start.X < window_Zoom_XY_End.X)
                {
                    wLeft = window_Zoom_XY_Start.X;
                    wWidth = window_Zoom_XY_End.X - window_Zoom_XY_Start.X;
                }
                else
                {
                    wLeft = window_Zoom_XY_End.X;
                    wWidth = window_Zoom_XY_Start.X - window_Zoom_XY_End.X;
                }

                float wTop;
                float wHeight;
                if (window_Zoom_XY_Start.Y < window_Zoom_XY_End.Y)
                {
                    wTop = window_Zoom_XY_Start.Y;
                    wHeight = window_Zoom_XY_End.Y - window_Zoom_XY_Start.Y;
                }
                else
                {
                    wTop = window_Zoom_XY_End.Y;
                    wHeight = window_Zoom_XY_Start.Y - window_Zoom_XY_End.Y;
                }


                Pen wPen = new Pen(zoom_Window_Frame.Color, zoom_Window_Frame.Width);
                gGraphArea.DrawRectangle(wPen, wLeft, wTop, wWidth, wHeight);
            }
        }
    }
}
