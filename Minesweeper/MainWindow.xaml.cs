using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/* DEVELOPER NOTE: To be implemented: Timer (optional)
 * Current status: Functional*/

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Sprites
        static readonly BitmapImage spriteSheet = new BitmapImage(new Uri("pack://application:,,,/minesweeperAssets.png"));

        //Sprites for each tile (the gameboard cells)
        readonly ImageSource sprtCellOne = new CroppedBitmap(spriteSheet, new Int32Rect(0, 66, 16, 16));
        readonly ImageSource sprtCellTwo = new CroppedBitmap(spriteSheet, new Int32Rect(17, 66, 16, 16));
        readonly ImageSource sprtCellThree = new CroppedBitmap(spriteSheet, new Int32Rect(34, 66, 16, 16));
        readonly ImageSource sprtCellFour = new CroppedBitmap(spriteSheet, new Int32Rect(51, 66, 16, 16));
        readonly ImageSource sprtCellFive = new CroppedBitmap(spriteSheet, new Int32Rect(68, 66, 16, 16));
        readonly ImageSource sprtCellSix = new CroppedBitmap(spriteSheet, new Int32Rect(85, 66, 16, 16));
        readonly ImageSource sprtCellSeven = new CroppedBitmap(spriteSheet, new Int32Rect(102, 66, 16, 16));
        readonly ImageSource sprtCellEight = new CroppedBitmap(spriteSheet, new Int32Rect(119, 66, 16, 16));
        readonly ImageSource sprtCellClosed = new CroppedBitmap(spriteSheet, new Int32Rect(0, 49, 16, 16));
        readonly ImageSource sprtCellEmpty = new CroppedBitmap(spriteSheet, new Int32Rect(17, 49, 16, 16));
        readonly ImageSource sprtCellFlagged = new CroppedBitmap(spriteSheet, new Int32Rect(34, 49, 16, 16));
        readonly ImageSource sprtCellMineUnclicked = new CroppedBitmap(spriteSheet, new Int32Rect(85, 49, 16, 16));
        readonly ImageSource sprtCellMineClicked = new CroppedBitmap(spriteSheet, new Int32Rect(102, 49, 16, 16));
        readonly ImageSource sprtCellFalseFlag = new CroppedBitmap(spriteSheet, new Int32Rect(119, 49, 16, 16));

        //Sprites for the face (The lil guy up top)
        readonly ImageSource sprtFaceSmile = new CroppedBitmap(spriteSheet, new Int32Rect(0, 24, 24, 24));
        readonly ImageSource sprtFaceSmilePressed = new CroppedBitmap(spriteSheet, new Int32Rect(25, 24, 24, 24));
        readonly ImageSource sprtFaceGasp = new CroppedBitmap(spriteSheet, new Int32Rect(50, 24, 24, 24));
        readonly ImageSource sprtFaceVictory = new CroppedBitmap(spriteSheet, new Int32Rect(75, 24, 24, 24));
        readonly ImageSource sprtFaceDeath = new CroppedBitmap(spriteSheet, new Int32Rect(100, 24, 24, 24));

        //Sprites for the mine count and timer
        readonly ImageSource sprtCounterOne = new CroppedBitmap(spriteSheet, new Int32Rect(0, 0, 13, 23));
        readonly ImageSource sprtCounterTwo = new CroppedBitmap(spriteSheet, new Int32Rect(14, 0, 13, 23));
        readonly ImageSource sprtCounterThree = new CroppedBitmap(spriteSheet, new Int32Rect(28, 0, 13, 23));
        readonly ImageSource sprtCounterFour = new CroppedBitmap(spriteSheet, new Int32Rect(42, 0, 13, 23));
        readonly ImageSource sprtCounterFive = new CroppedBitmap(spriteSheet, new Int32Rect(56, 0, 13, 23));
        readonly ImageSource sprtCounterSix = new CroppedBitmap(spriteSheet, new Int32Rect(70, 0, 13, 23));
        readonly ImageSource sprtCounterSeven = new CroppedBitmap(spriteSheet, new Int32Rect(84, 0, 13, 23));
        readonly ImageSource sprtCounterEight = new CroppedBitmap(spriteSheet, new Int32Rect(98, 0, 13, 23));
        readonly ImageSource sprtCounterNine = new CroppedBitmap(spriteSheet, new Int32Rect(112, 0, 13, 23));
        readonly ImageSource sprtCounterZero = new CroppedBitmap(spriteSheet, new Int32Rect(126, 0, 13, 23));
        readonly ImageSource sprtCounterHyphen = new CroppedBitmap(spriteSheet, new Int32Rect(140, 0, 13, 23));
        readonly ImageSource sprtCounterBlank = new CroppedBitmap(spriteSheet, new Int32Rect(154, 0, 13, 23));
        #endregion

        bool isLoaded = false;
        bool gameIsLost = false;
        bool gameIsWon = false;
        int boardWidth;
        int boardHeight;
        int cellsOpened;
        int flagCount;
        int mineCount;
        List<List<int>> boardList;
        Dictionary<Tuple<int, int>, Button> boardDictionary = [];

        public MainWindow()
        {
            InitializeComponent();

            btnRestartButton.Content = new Image { Source = sprtFaceSmile, Width = 24, Height = 24, Stretch = Stretch.None };

        }
        public enum Difficulty { Easy = 10, Medium = 40, Hard = 99 };
        public enum CellState {  Flagged = -2, Closed, Open, One, Two, Three, Four, Five, Six, Seven, Eight};
        


        private void Generate_Gameboard(int mineCountLocal)
        {
            if (mineCountLocal == (int)Difficulty.Easy) { boardWidth = 9; boardHeight = 9; }
            else if (mineCountLocal == (int)Difficulty.Medium) { boardWidth = 16; boardHeight = 16; }
            else if (mineCountLocal == (int)Difficulty.Hard) { boardWidth = 32; boardHeight = 16; }

            cellsOpened = 0;
            flagCount = 0;            
            gameIsLost = false;
            gameIsWon = false;
            mineCount = mineCountLocal;
            Image restartButtonImage = (Image)btnRestartButton.Content;
            restartButtonImage.Source = sprtFaceSmile;

            Initialize_GameboardUI();

            //Creates a temp 2D list of mines and uses the list to make boardList (number/mine index)
            // -1 is a Mine. Column (second index) is X. Row (first index) is Y.
            Random rnd = new Random();
            List<List<bool>> mineList = new(boardHeight);
            boardList = new List<List<int>>(boardHeight);
            for (int rowsMade = 0; rowsMade < boardHeight; rowsMade++)
            {
                mineList.Add(Enumerable.Repeat(false, boardWidth).ToList());
                boardList.Add(Enumerable.Repeat(0, boardWidth).ToList());
            }

            for (int count = 0; count < mineCountLocal;) 
            {
                int row = rnd.Next(0, boardHeight);
                int column = rnd.Next(0, boardWidth);
                if (!mineList[row][column])
                {
                    mineList[row][column] = true;
                    count++;
                }
            }

            // Use mineList to calculate number/mine status of cells, stored in boardList
            for (int row = 0; row < boardHeight; row++)
            {
                for (int column = 0; column < boardWidth; column++)
                {
                    if (mineList[row][column])
                    {
                        boardList[row][column] = -1;
                    }
                    else
                    {
                        int adjacentMines = 0;
                        for (int adjacentRow = row - 1; adjacentRow <= row + 1; adjacentRow++)
                        {
                            for (int adjacentColumn = column - 1; adjacentColumn <= column + 1; adjacentColumn++)
                            {
                                if (adjacentRow < 0 || adjacentRow >= boardHeight || adjacentColumn < 0 || adjacentColumn >= boardWidth)
                                    continue;
                                if (mineList[adjacentRow][adjacentColumn])
                                {
                                    adjacentMines++;
                                }
                            }
                        }
                        boardList[row][column] = adjacentMines;
                    }
                }
            }
            UpdateFlagCounter();
        }
        
        ///Changes window size, grid size, and initializes cells.
        private void Initialize_GameboardUI()
        {
            //Changes window size and grid size
            this.Width = (boardWidth * 16) + 20 ;
            this.Height = (boardHeight * 16) + 70;
            gridGameBoard.Height = boardHeight * 16;
            gridGameBoard.Width = boardWidth * 16;
            gridGameBoard.Children.Clear();

            //Adds or removes rows and columns to reach the desired size
            while (gridGameBoard.RowDefinitions.Count > boardHeight) { gridGameBoard.RowDefinitions.RemoveAt(gridGameBoard.RowDefinitions.Count - 1); }
            while (gridGameBoard.RowDefinitions.Count < boardHeight) { gridGameBoard.RowDefinitions.Add(new()); }
            while (gridGameBoard.ColumnDefinitions.Count > boardWidth) { gridGameBoard.ColumnDefinitions.RemoveAt(gridGameBoard.ColumnDefinitions.Count - 1); }
            while (gridGameBoard.ColumnDefinitions.Count < boardWidth) {  gridGameBoard.ColumnDefinitions.Add(new()); }

            for (int i = 0; i < boardWidth * boardHeight; i++)
            {
                int row = i / boardWidth;
                int column = i % boardWidth;
                Button btnNewCell = new();
                //Creates, adds, and manages setting for the new cell
                gridGameBoard.Children.Add( btnNewCell );
                btnNewCell.Tag = (int) CellState.Closed;
                btnNewCell.Content = new Image{ Source = sprtCellClosed, Width = 16, Height = 16, Stretch = Stretch.None};
                btnNewCell.BorderThickness = new Thickness(0);
                btnNewCell.Padding = new Thickness(0);
                Grid.SetRow(btnNewCell, row);
                Grid.SetColumn(btnNewCell, column);
                //Dictionary allows for reference to cell object using coordinate key
                Tuple<int,int> dictKey = new Tuple<int, int>(row,column);
                boardDictionary[dictKey] = btnNewCell;
                //Adds events to cells
                btnNewCell.Click += Cell_Clicked;
                btnNewCell.MouseRightButtonDown += Cell_RightMouseDown;
                btnNewCell.PreviewMouseLeftButtonDown += Cell_PreviewLeftMouseDown;
                btnNewCell.PreviewMouseLeftButtonUp += Cell_PreviewLeftMouseUp;
            }
        }

        private void OpenAdjacentCells(Button sender)
            /// Recursive function. Used for cascade openings (when an empty cell is clicked)
        {
            int senderRow = Grid.GetRow(sender);
            int senderColumn = Grid.GetColumn(sender);
            for (int row = senderRow - 1; row <= senderRow + 1; row++)
            {
                for (int column = senderColumn - 1; column <= senderColumn + 1; column++)
                {
                    if (row < 0 || row >= boardHeight || column < 0 || column >= boardWidth)
                    {
                        continue;
                    }
                Button selectedButton = boardDictionary[new Tuple<int, int> (row, column)];
                if ((int)selectedButton.Tag == (int)CellState.Closed)
                    {
                        Cell_Clicked(selectedButton, new RoutedEventArgs());
                        if (boardList[row][column] == 0)
                        {
                            OpenAdjacentCells(selectedButton);
                        }
                    }
                }
                
            }
        }
private void RevealMines()
        {
            foreach (Button cell in gridGameBoard.Children)
            {
                int row = Grid.GetRow(cell);
                int column = Grid.GetColumn(cell);
                Image img = (Image)cell.Content;
                if (boardList[row][column] == -1)
                {
                    switch ((int)cell.Tag)
                    {
                        case (int)CellState.Closed:
                            img.Source = sprtCellMineUnclicked;
                            break;
                        case (int)CellState.Flagged:
                            break;
                    }
                }
                else
                {
                    if ((int)cell.Tag == (int)CellState.Flagged)
                    {
                        img.Source = sprtCellFalseFlag;
                    }
                }
            }
        }

        private void UpdateFlagCounter()
        {
            int minesRemaining = mineCount - flagCount;
            string strMinesRemaining = Convert.ToString(minesRemaining);
            /* Cases:
             * Less than -99: display as dash-dash-dash
             * -99 to -10: display as dash-num-num
             * -9 to -1: display as blank-dash-num
             * 0 to 9: display as blank-blank-num
             * 10 to 99: display as blank-num-num
             * 99+ (impossible): display as num-num-num*/
            Debug.Assert(minesRemaining <= 99, "Mine count is impossible \t Assertion from: UpdateFlagCounter()");
            if (minesRemaining >= 10)
            {
                imgFlagDigit1.Source = sprtCounterBlank;
                imgFlagDigit2.Source = ConvertCountToImage(strMinesRemaining[0]);
                imgFlagDigit3.Source = ConvertCountToImage(strMinesRemaining[1]);
            }
            else if (minesRemaining >= 0)
            {
                imgFlagDigit1.Source = sprtCounterBlank;
                imgFlagDigit2.Source = sprtCounterBlank;
                imgFlagDigit3.Source = ConvertCountToImage(strMinesRemaining[0]);
            }
            else if (minesRemaining >= -9)
            {
                imgFlagDigit1.Source = sprtCounterBlank;
                imgFlagDigit2.Source = sprtCounterHyphen;
                imgFlagDigit3.Source = ConvertCountToImage(strMinesRemaining[1]);
            }
            else if (minesRemaining >= -99)
            {
                imgFlagDigit1.Source = sprtCounterHyphen;
                imgFlagDigit2.Source = ConvertCountToImage(strMinesRemaining[1]);
                imgFlagDigit3.Source = ConvertCountToImage(strMinesRemaining[2]);
            }
            else
            {
                imgFlagDigit1.Source = sprtCounterHyphen;
                imgFlagDigit2.Source = sprtCounterHyphen;
                imgFlagDigit3.Source = sprtCounterHyphen;
            }
        }

        /// <summary>
        /// Solely made to avoid repeating long switch statements in UpdateFlagCounter()
        /// </summary>
        private ImageSource ConvertCountToImage(char digit)
        {
            switch (digit)
            {
                case '0':
                    return sprtCounterZero;
                case '1':
                    return sprtCounterOne;
                case '2':
                    return sprtCounterTwo;
                case '3':
                    return sprtCounterThree;
                case '4':
                    return sprtCounterFour;
                case '5':
                    return sprtCounterFive;
                case '6':
                    return sprtCounterSix;
                case '7':
                    return sprtCounterSeven;
                case '8':
                    return sprtCounterEight;
                case '9':
                    return sprtCounterNine;
                default:
                    throw new Exception("An invalid input was given to ConvertCountToImage");
            }
        }

        #region Event Handler Methods
        private void Cell_Clicked(object sender, RoutedEventArgs e)
        {
            if (gameIsLost || gameIsWon) { return; }
            Button btn = (Button)sender;
            int row = Grid.GetRow(btn);
            int column = Grid.GetColumn(btn);
            if ( (int)btn.Tag == (int)CellState.Closed)
            {
                cellsOpened++;
                Image img = (Image)btn.Content;
                Image restartButtonImage = (Image)btnRestartButton.Content;
                switch (boardList[row][column])
                {
                    case -1:
                        cellsOpened--;
                        gameIsLost = true;
                        restartButtonImage.Source = sprtFaceDeath;
                        RevealMines();
                        img.Source = sprtCellMineClicked;
                        break;
                    case 0:
                        img.Source = sprtCellEmpty;
                        btn.Tag = (int)CellState.Open;
                        OpenAdjacentCells(btn);
                        break;
                    case 1:
                        img.Source = sprtCellOne;
                        btn.Tag = (int)CellState.One;
                        break;
                    case 2:
                        img.Source = sprtCellTwo;
                        btn.Tag = (int)CellState.Two;
                        break;
                    case 3:
                        img.Source = sprtCellThree;
                        btn.Tag = (int)CellState.Three;
                        break;
                    case 4:
                        img.Source = sprtCellFour;
                        btn.Tag = (int)CellState.Four;
                        break;
                    case 5:
                        img.Source = sprtCellFive;
                        btn.Tag =(int)CellState.Five;
                        break;
                    case 6:
                        img.Source = sprtCellSix;
                        btn.Tag = (int)CellState.Six;
                        break;
                    case 7:
                        img.Source = sprtCellSeven;
                        btn.Tag = (int)CellState.Seven;
                        break;
                    case 8:
                        img.Source = sprtCellEight;
                        btn.Tag = (int)CellState.Eight;
                        break;
                }
            
            if (cellsOpened == (boardWidth*boardHeight) - mineCount)
                {
                    gameIsWon = true;
                    restartButtonImage.Source = sprtFaceVictory;
                }
            }
            /* Testing material. Should be changed for final version. Changes any clicked square to 1.
            btn.Tag = (int)CellState.One;
            Use a match statement and premade master board to determine sprite
            btn.Content = new Image { Source = sprtCellOne, Width = 16, Height = 16, Stretch = Stretch.None }; */
        }


        private void Cell_RightMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gameIsLost || gameIsWon) { return; }

            Button btn = (Button)sender;
            Image img = (Image)btn.Content;
            int tag = (int)btn.Tag;

            if (tag == (int)CellState.Closed)
            {
                flagCount++;
                img.Source = sprtCellFlagged;
                btn.Tag = (int)CellState.Flagged;
                UpdateFlagCounter();
            }
            else if (tag == (int)CellState.Flagged)
            {
                flagCount--;
                img.Source = sprtCellClosed;
                btn.Tag = (int)CellState.Closed;
                UpdateFlagCounter();
            }
        }

        private void DifficultyMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoaded)
            {
                ComboBoxItem item = (ComboBoxItem)DifficultyMenu.SelectedItem;
                int mine_count = Convert.ToInt32(item.Tag);
                Generate_Gameboard(mine_count);
            }
        }

        ///When window first loads, generates an Easy board
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
            Generate_Gameboard((int)Difficulty.Easy);
        }

        /* All Preview event handlers update button appearance when held/unheld */
        private void Cell_PreviewLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gameIsLost || gameIsWon) { return; }
            Image restartButtonImage = (Image)btnRestartButton.Content;
            restartButtonImage.Source = sprtFaceGasp;
        }
        private void Cell_PreviewLeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (gameIsLost || gameIsWon) { return; }
            Image restartButtonImage = (Image)btnRestartButton.Content;
            restartButtonImage.Source = sprtFaceSmile;
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoaded)
            {
                ComboBoxItem item = (ComboBoxItem)DifficultyMenu.SelectedItem;
                int mine_count = Convert.ToInt32(item.Tag);
                Generate_Gameboard(mine_count);
            }
        }
        private void RestartButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image restartButtonImage = (Image)btnRestartButton.Content;
            restartButtonImage.Source = sprtFaceSmilePressed;
        }

        private void RestartButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image restartButtonImage = (Image)btnRestartButton.Content;
            if (gameIsLost)
            {
                restartButtonImage.Source = sprtFaceDeath;
            }
            else if (gameIsWon)
            {
                restartButtonImage.Source = sprtFaceVictory;
            }
            else
            {
                restartButtonImage.Source = sprtFaceSmile;
            }
        }
        #endregion


    }
}