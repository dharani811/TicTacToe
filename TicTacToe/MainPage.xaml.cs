using Microsoft.Maui.Controls.Shapes;
using TicTacToe.GameLogic;

namespace TicTacToe;

public partial class MainPage : ContentPage
{
    private readonly Dictionary<Player, ImageSource> imageSources = new Dictionary<Player, ImageSource>
        {
            { Player.X , null },
            { Player.O , null  },
            { Player.None, null }
        };

    private readonly GameLogic.GameLogic gameLogic = new GameLogic.GameLogic();
    private readonly Dictionary<Tuple<int, int>, int> grid = new()
    {
        {new(0,0),0 },
        {new(0,1),1 },
        {new(0,2),2 },
        {new(1,0),3 },
        {new(1,1),4 },
        {new(1,2),5 },
        {new(2,0),6 },
        {new(2,1),7 },
        {new(2,2),8 }
    };
    public MainPage()
	{
		InitializeComponent();
        imageSources[Player.X] = ImageSource.FromFile("xother.png");
        imageSources[Player.O] = ImageSource.FromFile("other.png");
        OnGameRestart();
    }


    private async void GameFlow(int r, int c)
    {
        if (gameLogic.PlayerMakeMove(r, c) && !gameLogic.GameOver)
        {
            OnMoveMade(r, c);
            await Task.Delay(400);
            gameLogic.AiMakeMove();
            if (!gameLogic.GameOver)
            {
                OnMoveMade(gameLogic.AiRow, gameLogic.AiColumn);
            }
            else
            {
                OnMoveMade(gameLogic.AiRow, gameLogic.AiColumn);
                OnGameEnded(gameLogic.GetGameResult());
            }
        }
        else if (gameLogic.GameOver)
        {
            OnMoveMade(r, c);
            OnGameEnded(gameLogic.GetGameResult());
        }
    }


    private void GameGrid_Click(Point clickPosition)
    {
        if (gameLogic.CurrentPlayer != Player.O) { return; }

        double squareSize = GameGrid.WidthRequest / 3;
        int row = (int)(clickPosition.Y / squareSize);
        int col = (int)(clickPosition.X / squareSize);

        GameFlow(row, col);
    }


    private (Point, Point) FindLinePoints(WinInfo winInfo)
    {
        double squareSize = GameGrid.Width / 3;
        double margin = squareSize / 2;

        if (winInfo.WinTypeKey == WinInfo.WinType.row)
        {
            double y = winInfo.WinTypeNum * squareSize + margin;
            return (new Point(0, y), new Point(GameGrid.Width, y));
        }
        if (winInfo.WinTypeKey == WinInfo.WinType.col)
        {
            double x = winInfo.WinTypeNum * squareSize + margin;
            return (new Point(x, 0), new Point(x, GameGrid.Height));
        }
        if (winInfo.WinTypeKey == WinInfo.WinType.mainDiag)
        {
            return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
        }

        return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height));

    }



    private void OnMoveMade(int r, int c)
    {
        Player player = gameLogic.GameGrid[r, c];
        PlayerImage.Source = imageSources[gameLogic.CurrentPlayer];
        (GameGrid.Children[grid[new(r, c)]] as Image).Source = imageSources[player];
    }


    private async void OnGameEnded(GameResult gameResult)
    {
        await Task.Delay(800);

        if (gameResult.GameWinner == Player.None)
        {
            TransitionToEndScreen("it's a draw", null);
        }
        else
        {
           // GameGrid.Children.All((x) => x. = Visibility.Hidden);
            //await Task.Delay(1000);
            TransitionToEndScreen("The Winner: ", imageSources[gameResult.GameWinner]);
        }
    }


    private async void OnGameRestart()
    {
        GameGrid.Children.AsParallel().ForAll(x => (x as Image).Source = ImageSource.FromFile("blank.png"));
        PlayerImage.Source = imageSources[gameLogic.CurrentPlayer];
        TransitionToGameScreen();
        gameLogic.AiMakeMove();
        OnMoveMade(gameLogic.AiRow, gameLogic.AiColumn);
    }

    private void TransitionToEndScreen(string text, ImageSource winnerImage)
    {
        TurnPanel.IsVisible = false;
        GameGrid.IsVisible = false;
        ResultText.Text = text;
        WinnerImage.Source = winnerImage;
        EndScreen.IsVisible = true;
    }


    private void TransitionToGameScreen()
    {
        GameGrid.IsVisible = true;
        EndScreen.IsVisible = false;
        TurnPanel.IsVisible = true;
        GameCanvas.IsVisible = true;
    }   

    private void Button_Clicked(object sender, EventArgs e)
    {
        gameLogic.Reset();
        OnGameRestart();

    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        GameGrid_Click((Point)e.GetPosition(GameGrid));
    }
}

