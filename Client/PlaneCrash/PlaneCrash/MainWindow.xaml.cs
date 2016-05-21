using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
using Wrapper;

namespace PlaneCrash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Direction PlaneDirection { get; set; }
        public NetworkStream Stream;
        public string ClientName { get; set; }

        public bool IsMyTurn { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            IsMyTurn = false;
            Connect("192.168.0.171");
        }

        void Connect(String server)
        {

            try
            {
                Int32 port = 9000;
                TcpClient client = new TcpClient(server, port);

                Stream = client.GetStream();

                Thread oThread = new Thread(Listen);
                oThread.Start();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        #region process server messages
        public void Listen()
        {
            while (true)
            {
                Byte[] data = new Byte[1024];

                Stream.Read(data, 0, data.Length);

                MessageWrapper responseData = ByteArrayToObject(data);


                if (responseData.Phase == MessageWrapper.Phases.SENDIP) {
                    ClientName = responseData.YourName;
                }

                if (responseData.Phase == MessageWrapper.Phases.ACKNOWLEDGE)
                {
                    if (responseData.ActivePlayer == ClientName)
                    {
                        IsMyTurn = true;
                    }
                    else
                    {
                        IsMyTurn = false;

                    }
                }

                if (responseData.Phase == MessageWrapper.Phases.ATACK)
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        foreach (var btn in SelfPlaneMap.Children)
                        {
                            var button = btn as CustomButton;
                            if (button.Uid == responseData.CellToHit.ToString())
                            {
                                if (button.IsHead == true)
                                {
                                    // to do
                                    button.Background = Brushes.Black;
                                    ;
                                }
                                else
                                {
                                    var bu = 4;
                                }
                            }
                        }

                    }));

                }

            }
        }

        public void SendMessage(MessageWrapper message)
        {
            Byte[] data = ObjectToByteArray(message);
            Stream.Write(data, 0, data.Length);
        }

        private MessageWrapper ByteArrayToObject(byte[] arrBytes)
        {
            MessageWrapper ReturnValue;
            using (var _MemoryStream = new MemoryStream(arrBytes))
            {
                IFormatter _BinaryFormatter = new BinaryFormatter();
                ReturnValue = (MessageWrapper)_BinaryFormatter.Deserialize(_MemoryStream);
            }
            return ReturnValue;
        }

        public byte[] ObjectToByteArray(MessageWrapper obj)
        {
            byte[] bytes;
            using (var _MemoryStream = new MemoryStream())
            {
                IFormatter _BinaryFormatter = new BinaryFormatter();
                _BinaryFormatter.Serialize(_MemoryStream, obj);
                bytes = _MemoryStream.ToArray();
            }
            return bytes;
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateMap(SelfPlaneMap, Brushes.Violet, true);
            CreateMap(HitTargetMap, Brushes.SkyBlue, false);
        }

        private void RotateLeft_Click(object sender, RoutedEventArgs e)
        {
            PlaneImage.RenderTransform = new RotateTransform(-90);
            PlaneDirection = Direction.Left;
        }

        private void RotateDownClick(object sender, RoutedEventArgs e)
        {
            PlaneImage.RenderTransform = new RotateTransform(180);
            PlaneDirection = Direction.Down;
        }

        private void RotateRightClick(object sender, RoutedEventArgs e)
        {
            PlaneImage.RenderTransform = new RotateTransform(90);
            PlaneDirection = Direction.Right;
        }

        private void RotateUpClick(object sender, RoutedEventArgs e)
        {
            PlaneImage.RenderTransform = new RotateTransform(0);
            PlaneDirection = Direction.Up;
        }

        private void CreateMap(Grid gridMap, Brush mapColor, bool allowDrop)
        {
            for (int rowIndex = 0; rowIndex < 10; rowIndex++)
            {
                RowDefinition row = new RowDefinition();
                gridMap.RowDefinitions.Add(row);
            }

            for (int columnIndex = 0; columnIndex < 10; columnIndex++)
            {
                ColumnDefinition colum = new ColumnDefinition();
                gridMap.ColumnDefinitions.Add(colum);
            }

            int k = 0;
            for (int rowIndex = 0; rowIndex < 10; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < 10; columnIndex++)
                {
                    k++;
                    CustomButton placeHolder = new CustomButton() { Content = k.ToString() };
                    placeHolder.Uid = k.ToString();
                    placeHolder.Drop += PlaceHolder_Drop;
                    placeHolder.DragEnter += PlaceHolder_DragEnter;
                    placeHolder.AllowDrop = allowDrop;
                    placeHolder.Background = mapColor;
                    placeHolder.Click += ButtonClick;

                    placeHolder.SetValue(Grid.RowProperty, rowIndex);
                    placeHolder.SetValue(Grid.ColumnProperty, columnIndex);

                    gridMap.Children.Add(placeHolder);
                }
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var cb = sender as CustomButton;
            var hitCellId = Convert.ToInt32(cb.Uid);
            SendMessage(new MessageWrapper() { CellToHit = hitCellId, Phase = MessageWrapper.Phases.ATACK });
        }

        private void PlaceHolder_DragEnter(object sender, DragEventArgs e)
        {
            ClearSelection();
            var btn = sender as CustomButton;
            int headId = Convert.ToInt32(btn.Content);
            var data = e.Data.GetData("Direction");

            DrawPlane((Direction)data, headId, true, Brushes.Red);
        }

        private void PlaceHolder_Drop(object sender, DragEventArgs e)
        {
            var btn = sender as CustomButton;
            int headId = Convert.ToInt32(btn.Content);
            var data = e.Data.GetData("Direction");

            DrawPlane((Direction)data, headId, false, Brushes.Yellow);
        }

        private void ClearSelection()
        {
            var btns = SelfPlaneMap.Children.Cast<UIElement>().ToList();
            foreach (var item in btns)
            {
                var btn = item as CustomButton;
                if (btn.Background == Brushes.Yellow && btn.IsSelected == true)
                {
                    btn.Background = Brushes.Yellow;
                }
                else if (btn.Background == Brushes.Red && btn.IsSelected == false)
                {
                    btn.Background = Brushes.Violet;
                }
            }
        }

        static public int NumberOfAddedPlanes = 0;

        private void DrawPlane(Direction planeDirection, int headId, bool preview, Brush planeColor)
        {
            try
            {
                switch (planeDirection)
                {
                    case Direction.Up:
                        PlaneCoordinates headUpCoord = GetHeadUpPlaneCoord(headId);
                        if (!InvalidCoordonateFound(headUpCoord))
                        {
                            CreatePlane(headUpCoord, planeColor, preview);
                            if (planeColor == Brushes.Yellow)
                            {
                                NumberOfAddedPlanes++;
                            }
                            if (NumberOfAddedPlanes == 3 && planeColor == Brushes.Yellow)
                            {
                                SendMessage(new MessageWrapper() { PlanesReady = true });
                            }
                        }
                        break;
                    case Direction.Down:
                        PlaneCoordinates headDownCoord = GetHeadDownPlaneCoord(headId);
                        if (!InvalidCoordonateFound(headDownCoord))
                        {
                            CreatePlane(headDownCoord, planeColor, preview);
                            if (planeColor == Brushes.Yellow)
                            {
                                NumberOfAddedPlanes++;
                            }
                            if (NumberOfAddedPlanes == 3 && planeColor == Brushes.Yellow)
                            {
                                SendMessage(new MessageWrapper() { PlanesReady = true });
                            }
                        }
                        break;
                    case Direction.Left:
                        PlaneCoordinates headLeftCoord = GetHeadLeftPlaneCoord(headId);
                        if (!InvalidCoordonateFound(headLeftCoord))
                        {
                            CreatePlane(headLeftCoord, planeColor, preview);
                            if (planeColor == Brushes.Yellow)
                            {
                                NumberOfAddedPlanes++;
                            }
                            if (NumberOfAddedPlanes == 3 && planeColor == Brushes.Yellow)
                            {
                                SendMessage(new MessageWrapper() { PlanesReady = true });
                            }
                        }
                        break;
                    case Direction.Right:
                        PlaneCoordinates headRightCoord = GetHeadRightPlaneCoord(headId);
                        if (!InvalidCoordonateFound(headRightCoord))
                        {
                            CreatePlane(headRightCoord, planeColor, preview);
                            if (planeColor == Brushes.Yellow)
                            {
                                NumberOfAddedPlanes++;
                            }
                            if (NumberOfAddedPlanes == 3)
                            {
                                SendMessage(new MessageWrapper() { PlanesReady = true });
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        private bool InvalidCoordonateFound(PlaneCoordinates planeCoordinates)
        {
            CustomButton headBtn = GetButtonById(planeCoordinates.HeadId);
            CustomButton cellCenterFrontBtn = GetButtonById(planeCoordinates.CellCenterFrontId);
            CustomButton cellLeftFrontWingBtn = GetButtonById(planeCoordinates.CellLeftFrontWingId);
            CustomButton cellRightFrontWingBtn = GetButtonById(planeCoordinates.CellRightFrontWingId);
            CustomButton cellCenterMiddleBtn = GetButtonById(planeCoordinates.CellCenterMiddleId);
            CustomButton cellCenterBackBtn = GetButtonById(planeCoordinates.CellCenterBackId);
            CustomButton cellLeftBackWingBtn = GetButtonById(planeCoordinates.CellLeftBackWingId);
            CustomButton cellRightBackWingBtn = GetButtonById(planeCoordinates.CellRightBackWingId);

            if (headBtn.IsSelected == true ||
                cellCenterFrontBtn.IsSelected == true ||
                cellLeftFrontWingBtn.IsSelected == true ||
                cellRightFrontWingBtn.IsSelected == true ||
                cellCenterMiddleBtn.IsSelected == true ||
                cellCenterBackBtn.IsSelected == true ||
                cellLeftBackWingBtn.IsSelected == true ||
                cellRightBackWingBtn.IsSelected == true)
            {
                return true;
            }
            return false;
        }

        private PlaneCoordinates GetHeadRightPlaneCoord(int headId)
        {
            PlaneCoordinates pCoord = new PlaneCoordinates();
            pCoord.HeadId = headId;
            pCoord.CellCenterFrontId = pCoord.HeadId - 1;
            pCoord.CellLeftFrontWingId = pCoord.CellCenterFrontId - 10;
            pCoord.CellRightFrontWingId = pCoord.CellCenterFrontId + 10;
            pCoord.CellCenterMiddleId = pCoord.CellCenterFrontId - 1;
            pCoord.CellCenterBackId = pCoord.CellCenterMiddleId - 1;
            pCoord.CellLeftBackWingId = pCoord.CellCenterBackId - 10;
            pCoord.CellRightBackWingId = pCoord.CellCenterBackId + 10;
            return pCoord;
        }

        private PlaneCoordinates GetHeadDownPlaneCoord(int headId)
        {
            PlaneCoordinates pCoord = new PlaneCoordinates();
            pCoord.HeadId = headId;
            pCoord.CellCenterFrontId = pCoord.HeadId - 10;
            pCoord.CellLeftFrontWingId = pCoord.CellCenterFrontId + 1;
            pCoord.CellRightFrontWingId = pCoord.CellCenterFrontId - 1;
            pCoord.CellCenterMiddleId = pCoord.CellCenterFrontId - 10;
            pCoord.CellCenterBackId = pCoord.CellCenterMiddleId - 10;
            pCoord.CellLeftBackWingId = pCoord.CellCenterBackId + 1;
            pCoord.CellRightBackWingId = pCoord.CellCenterBackId - 1;
            return pCoord;
        }

        private PlaneCoordinates GetHeadLeftPlaneCoord(int headId)
        {
            PlaneCoordinates pCoord = new PlaneCoordinates();
            pCoord.HeadId = headId;
            pCoord.CellCenterFrontId = pCoord.HeadId + 1;
            pCoord.CellLeftFrontWingId = pCoord.CellCenterFrontId + 10;
            pCoord.CellRightFrontWingId = pCoord.CellCenterFrontId - 10;
            pCoord.CellCenterMiddleId = pCoord.CellCenterFrontId + 1;
            pCoord.CellCenterBackId = pCoord.CellCenterMiddleId + 1;
            pCoord.CellLeftBackWingId = pCoord.CellCenterBackId + 10;
            pCoord.CellRightBackWingId = pCoord.CellCenterBackId - 10;
            return pCoord;
        }

        private PlaneCoordinates GetHeadUpPlaneCoord(int headId)
        {
            PlaneCoordinates pCoord = new PlaneCoordinates();
            pCoord.HeadId = headId;
            pCoord.CellCenterFrontId = pCoord.HeadId + 10;
            pCoord.CellLeftFrontWingId = pCoord.CellCenterFrontId - 1;
            pCoord.CellRightFrontWingId = pCoord.CellCenterFrontId + 1;
            pCoord.CellCenterMiddleId = pCoord.CellCenterFrontId + 10;
            pCoord.CellCenterBackId = pCoord.CellCenterMiddleId + 10;
            pCoord.CellLeftBackWingId = pCoord.CellCenterBackId - 1;
            pCoord.CellRightBackWingId = pCoord.CellCenterBackId + 1;
            return pCoord;
        }

        private void CreatePlane(PlaneCoordinates planeCoordinates, Brush planeColor, bool preview)
        {
            CustomButton headBtn = GetButtonById(planeCoordinates.HeadId);
            if (headBtn != null)
            {
                headBtn.Background = planeColor;
                headBtn.IsHead = true;

                CustomButton cellCenterFrontBtn = GetButtonById(planeCoordinates.CellCenterFrontId);
                cellCenterFrontBtn.Background = planeColor;

                CustomButton cellLeftFrontWingBtn = GetButtonById(planeCoordinates.CellLeftFrontWingId);
                cellLeftFrontWingBtn.Background = planeColor;

                CustomButton cellRightFrontWingBtn = GetButtonById(planeCoordinates.CellRightFrontWingId);
                cellRightFrontWingBtn.Background = planeColor;

                CustomButton cellCenterMiddleBtn = GetButtonById(planeCoordinates.CellCenterMiddleId);
                cellCenterMiddleBtn.Background = planeColor;

                CustomButton cellCenterBackBtn = GetButtonById(planeCoordinates.CellCenterBackId);
                cellCenterBackBtn.Background = planeColor;

                CustomButton cellLeftBackWingBtn = GetButtonById(planeCoordinates.CellLeftBackWingId);
                cellLeftBackWingBtn.Background = planeColor;

                CustomButton cellRightBackWingBtn = GetButtonById(planeCoordinates.CellRightBackWingId);
                cellRightBackWingBtn.Background = planeColor;

                if (!preview)
                {
                    headBtn.IsSelected =
                    cellCenterFrontBtn.IsSelected =
                    cellLeftFrontWingBtn.IsSelected =
                    cellRightFrontWingBtn.IsSelected =
                    cellCenterMiddleBtn.IsSelected =
                    cellCenterBackBtn.IsSelected =
                    cellLeftBackWingBtn.IsSelected =
                    cellRightBackWingBtn.IsSelected = true;
                }
                else
                {
                    headBtn.IsSelected =
                    cellCenterFrontBtn.IsSelected =
                    cellLeftFrontWingBtn.IsSelected =
                    cellRightFrontWingBtn.IsSelected =
                    cellCenterMiddleBtn.IsSelected =
                    cellCenterBackBtn.IsSelected =
                    cellLeftBackWingBtn.IsSelected =
                    cellRightBackWingBtn.IsSelected = false;
                }
            }
        }

        private CustomButton GetButtonById(int id)
        {
            var btns = SelfPlaneMap.Children.Cast<UIElement>().ToList();
            var foundBtn = btns.Where(b => b.Uid == id.ToString()).FirstOrDefault();
            if (foundBtn != null)
            {
                return foundBtn as CustomButton;
            }
            return null;
        }

        private void Plane_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            DataObject dragObjectData = new DataObject("Direction", PlaneDirection);
            DragDrop.DoDragDrop(image, dragObjectData, DragDropEffects.Move);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var uiCollection = SelfPlaneMap.Children.Cast<UIElement>().ToList();
            foreach (var item in uiCollection)
            {
                CustomButton btn = item as CustomButton;
                btn.Background = Brushes.Violet;
                btn.IsSelected = false;
            }
        }
    }

    public enum Direction
    {
        Up, Left, Right, Down
    }

    public class PlaneCoordinates
    {
        public int HeadId { get; set; }
        public int CellCenterFrontId { get; set; }
        public int CellLeftFrontWingId { get; set; }
        public int CellRightFrontWingId { get; set; }
        public int CellCenterMiddleId { get; set; }
        public int CellCenterBackId { get; set; }
        public int CellLeftBackWingId { get; set; }
        public int CellRightBackWingId { get; set; }
    }
}