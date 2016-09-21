using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinLifeGame
{
	public class App : Application
	{
		Cell[,] Cells;

		bool isExecuted = false;
		int CellCount = 13;
		GridLength CellSize = new GridLength(20);

		public App()
		{
			var RowDefinitions = new RowDefinition[CellCount];
			var RowCollections = new RowDefinitionCollection();
			for (int i = 0; i < CellCount; i++)
			{
				RowDefinitions[i] = new RowDefinition { Height = CellSize };
				RowCollections.Add(RowDefinitions[i]);
			}

			var ColumnDefinitions = new ColumnDefinition[CellCount];
			var ColumnCollections = new ColumnDefinitionCollection();
			for (int i = 0; i < CellCount; i++)
			{
				ColumnDefinitions[i] = new ColumnDefinition { Width = CellSize };
				ColumnCollections.Add(ColumnDefinitions[i]);
			}

			var CellBoard = new Grid
			{
				Padding = new Thickness(5, Device.OnPlatform(20, 0, 0), 5, 0),
				RowDefinitions = RowCollections,
				ColumnDefinitions = ColumnCollections
			};

			Cells = new Cell[CellCount, CellCount];

			var tgr = new TapGestureRecognizer();
			tgr.Tapped += (sender, e) => CellClicked(sender, e);

			for (int i = 0; i < CellCount; i++)
			{
				for (int j = 0; j < CellCount; j++)
				{
					Cells[i, j] = new Cell
					{
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center,
						State = 0,
						IndexX = i,
						IndexY = j
					};
					CellBoard.Children.Add(Cells[i, j], i, j);

					Cells[i, j].GestureRecognizers.Add(tgr);
				}
			}

			var StartButton = new Button
			{
				Text = "START",
				WidthRequest = 100,
				BackgroundColor = Color.White
			};
			var StopButton = new Button
			{
				Text = "STOP",
				WidthRequest = 100,
				BackgroundColor = Color.White
			};
			StartButton.Clicked += ControlButtonClicked;
			StartButton.Clicked += ControlButtonClicked;
			StopButton.Clicked += ControlButtonClicked;

			// The root page of your application
			var content = new ContentPage
			{
				Title = "XamarinSample2",
				Content = new StackLayout
				{
					Children = {
						new Label {
							Text = "Life Game",
							WidthRequest = 200,
							HeightRequest = 50,
							HorizontalOptions  = LayoutOptions.CenterAndExpand,
							VerticalOptions  = LayoutOptions.CenterAndExpand,
							BackgroundColor = Color.White
						},
						CellBoard,
						StartButton,
						StopButton
				}
				},
				BackgroundColor = Color.Gray
			};

			MainPage = content;
		}

		private void CellClicked(object sender, EventArgs e)
		{
			if (isExecuted)
			{
				return;
			}
			Cell cell = (Cell)sender;

			Debug.WriteLine("Cell Clicked : " + cell);

			if (cell.State == 0)
			{
				cell.State = 1;
			}
			else {
				cell.State = 0;
			}
		}

		private async void ControlButtonClicked(object sender, EventArgs e)
		{
			Debug.WriteLine("ControlButtonClicked start");

			Button button = (Button)sender;

			Debug.WriteLine("Button Clicked : " + button);

			if ("START".Equals(button.Text))
			{
				if (isExecuted)
				{
					return;
				}

				isExecuted = true;
				await StartGame();
			}
			else {
				isExecuted = false;
			}

			Debug.WriteLine("ControlButtonClicked end");
		}

		private void initializeCell()
		{
			Cells[4, 6].State = 1;
			Cells[5, 5].State = 1;
			Cells[6, 5].State = 1;
			Cells[7, 5].State = 1;
			Cells[8, 5].State = 1;

			Cells[9, 6].State = 1;
			Cells[5, 7].State = 1;
			Cells[6, 7].State = 1;
			Cells[7, 7].State = 1;
			Cells[8, 7].State = 1;
		}

		private async Task StartGame()
		{
			Debug.WriteLine("StartGame");

			var token = new CancellationToken();
			await Task.Run(async () =>
			{
				token.ThrowIfCancellationRequested();

				await Task.Delay(1000);
				while (isExecuted)
				{
					await ExecuteUpdateCells();
				}
			}, token);
		}

		private Task<object> ExecuteUpdateCells()
		{
			Debug.WriteLine("ExecuteUpdateCells : " + DateTime.Now.ToLocalTime().ToString("F"));

			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			Device.BeginInvokeOnMainThread(() =>
			{
				try
				{
					UpdateCells();
					tcs.SetResult(null);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});

			return tcs.Task;
		}

		private void UpdateCells()
		{
			Debug.WriteLine("UpdateCells");

			int[,] temp = new int[CellCount, CellCount];

			for (int i = 0; i < CellCount; i++)
			{
				for (int j = 0; j < CellCount; j++)
				{
					temp[i, j] = JudgeNextLife(i, j);
				}
			}

			for (int i = 0; i < CellCount; i++)
			{
				for (int j = 0; j < CellCount; j++)
				{
					Cells[i, j].State = temp[i, j];
				}
			}
		}

		private int JudgeNextLife(int x, int y)
		{
			int NeighborsCount = 0;

			if (x > 0)
			{
				if (Cells[x - 1, y].State == 1)
				{
					NeighborsCount++;
				}
				if (y > 0 && Cells[x - 1, y - 1].State == 1)
				{
					NeighborsCount++;
				}
				if (y < CellCount - 1 && Cells[x - 1, y + 1].State == 1)
				{
					NeighborsCount++;
				}
			}
			if (x < CellCount - 1)
			{
				if (Cells[x + 1, y].State == 1)
				{
					NeighborsCount++;
				}
				if (y > 0 && Cells[x + 1, y - 1].State == 1)
				{
					NeighborsCount++;
				}
				if (y < CellCount - 1 && Cells[x + 1, y + 1].State == 1)
				{
					NeighborsCount++;
				}
			}
			if (y > 0)
			{
				if (Cells[x, y - 1].State == 1)
				{
					NeighborsCount++;
				}
			}
			if (y < CellCount - 1)
			{
				if (Cells[x, y + 1].State == 1)
				{
					NeighborsCount++;
				}
			}

			int Life = Cells[x, y].State;
			int NewLife = 0;

			if (Life == 1)
			{
				if (NeighborsCount == 3 || NeighborsCount == 2)
				{
					NewLife = 1;
				}
			}
			else
			{
				if (NeighborsCount == 3)
				{
					NewLife = 1;
				}
			}

			Debug.WriteLine("Cell : x[" + x + "] y[" + y + "] Neighbor[" + NeighborsCount + "] Life[" + NewLife + "]");

			return NewLife;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
			initializeCell();
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}

	class Cell : Label
	{
		public int IndexY
		{
			get;
			set;
		}

		public int IndexX
		{
			get;
			set;
		}

		private int state;
		public int State
		{
			get
			{
				return this.state;
			}
			set
			{
				this.state = value;
				if (this.state == 1)
				{
					BackgroundColor = Color.Black;
				}
				else
				{
					BackgroundColor = Color.White;
				}
			}
		}
	}
}
