using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace L100 {
    public class Game : Form {
        private readonly Form _form;
        private int _index;
        private readonly List<Cell> _cells = new List<Cell>();
        private List<Cell> _clickableCells = new List<Cell>();
        private Cell _lastClickedCell;
        private int gridSize = 5;
        int ButtonWidth = 55;
        int ButtonHeight = 55;
        int marginDistance = 20;
        int start_x = 0;
        int start_y = 0;

        private bool automatic = true;

        private bool _isOver = false;

        private Color _backgroundColor = Color.FromArgb(32, 32, 32);
        private Color _cellColor = Color.FromArgb(48, 48, 48);
        private Color _cellCurrentColor = Color.FromArgb(0, 0, 150);
        private Color _cellClickedColor = Color.FromArgb(40, 40, 40);
        private Color _cellClickableColor = Color.FromArgb(0, 100, 0);
        private Color _textColor = Color.FromArgb(240, 240, 240);

        public Game(Form form) {
            _form = form;
            _form.AutoScaleMode = AutoScaleMode.None;
            _form.Height = 2 * marginDistance + gridSize * ButtonHeight + 39;
            _form.Width = 2 * marginDistance + gridSize * ButtonWidth + 16;
            _form.BackColor = _backgroundColor;
            _form.ForeColor = _textColor;
            _form.Font = new Font("Century Gothic", 14);
        }

        public void Start() {
            for (int x = 0; x < gridSize; x++) {
                for (int y = 0; y < gridSize; y++) {
                    Button button = new Button {
                        Top = start_x + (x * ButtonHeight + marginDistance),
                        Left = start_y + (y * ButtonWidth + marginDistance),
                        Width = ButtonWidth,
                        Height = ButtonHeight
                    };
                    button.Click += cellClick;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.BackColor = _cellColor;

                    _cells.Add(new Cell(button, x, y, start_x + (x * ButtonHeight + marginDistance),
                        start_y + (y * ButtonWidth + marginDistance)));
                    _form.Controls.Add(button);
                }
            }
        }

        private Cell getCellByCoords(int x, int y) {
            foreach (var cell in _cells) {
                if (cell.PosX == x && cell.PosY == y) {
                    return cell;
                }
            }

            return null;
        }

        private List<Cell> GetClickableCells(Cell srcCell) {
            List<Cell> clickableCells = new List<Cell>();

            foreach (var cell in _cells) {
                if (cell.IsClickable(srcCell)) {
                    clickableCells.Add(cell);
                }
            }

            return clickableCells;
        }

        private void Lose() {

        }

        private void Win() {

        }

        private void Reset() {
            ResetColors();
            _index = 0;
            _lastClickedCell.Reset();
            _lastClickedCell = null;
        }

        private void Undo() {
            if (_index <= 1) {
                Reset();
                return;
            }

            _lastClickedCell.Reset();
            _index--;
            foreach (var cell in _cells) {
                if (cell.Button.Text == _index.ToString()) {
                    _lastClickedCell = cell;
                    break;
                }
            }

            if (!automatic)
                UpdateColors(_lastClickedCell);
        }

        private void UpdateColors(Cell pressedCell) {
            ResetColors();

            if (automatic) {
                foreach (var cell in _cells) {
                    if (cell.Button.Text != "") {
                        int color = int.Parse(cell.Button.Text) * 255 / (gridSize * gridSize);
                        int textColor = 255 - color;
                        if (Math.Abs(color - textColor) < 50) {
                            textColor += 50;
                        }

                        cell.Button.BackColor = Color.FromArgb(color, color, color);
                        cell.Button.ForeColor = Color.FromArgb(textColor, textColor, textColor);
                    }
                }

                return;
            }

            foreach (var clickableCell in _clickableCells) {
                if (clickableCell.Button.Text == "") {
                    clickableCell.Button.BackColor = _cellColor;
                } else {
                    clickableCell.Button.BackColor = _cellClickedColor;
                }
            }

            if (_lastClickedCell != null) {
                _lastClickedCell.Button.BackColor = _cellClickedColor;
            }

            _clickableCells = GetClickableCells(pressedCell);
            foreach (var clickableCell in _clickableCells) {
                clickableCell.Button.BackColor = _cellClickableColor;
            }

            pressedCell.Button.BackColor = _cellCurrentColor;
        }

        private void ResetColors() {
            foreach (var cell in _cells) {
                if (cell.Button.Text == "")
                    cell.Button.BackColor = _cellColor;
            }
        }

        private void cellClick(object sender, EventArgs e) {
            Button srcButton = sender as Button;
            Cell cell = getCellByCoords(srcButton.Top, srcButton.Left);

            if (cell == _lastClickedCell) {
                Undo();
                return;
            }

            if (!cell.IsClickable(_lastClickedCell)) {
                return;
            }

            if (srcButton.Text != "") {
                return;
            }

            _index++;
            srcButton.Text = _index.ToString();
            if (!automatic)
                UpdateColors(cell);
            _lastClickedCell = cell;

            if (_index == gridSize * gridSize) {
                _isOver = true;
                Win();
                return;
            }

            if (automatic) {
                List<Cell> cells = GetClickableCells(cell);
                foreach (var clickableCell in cells) {
                    clickableCell.Button.PerformClick();
                    if (_isOver) {
                        UpdateColors(clickableCell);
                        return;
                    }

                    Undo();
                }
            } else {
                if (_clickableCells.Count == 0) {
                    Lose();
                }
            }
        }
    }

    public class Cell {
        public Button Button { get; }
        public int X { get; }
        public int Y { get; }
        public int PosX { get; }
        public int PosY { get; }

        private Color _cellColor = Color.FromArgb(48, 48, 48);
        private Color _cellCurrentColor = Color.FromArgb(0, 0, 150);
        private Color _cellClickedColor = Color.FromArgb(40, 40, 40);
        private Color _cellClickableColor = Color.FromArgb(0, 100, 0);

        public Cell(Button button, int x, int y, int posX, int posY) {
            Button = button;
            X = x;
            Y = y;
            PosX = posX;
            PosY = posY;
        }

        public void Reset() {
            Button.Text = "";
            Button.BackColor = _cellColor;
        }

        public static int operator -(Cell a, Cell b) {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        public bool IsClickable(Cell srcCell) {
            if (srcCell == null) {
                return true;
            }

            if (Button.Text != "") {
                return false;
            }

            return this - srcCell == 5;
        }
    }

    public partial class MainWindow : Form {
        private readonly Game game;

        public MainWindow() {
            InitializeComponent();
            game = new Game(this);
        }

        private void MainWindow_Load(object sender, EventArgs e) {
            game.Start();
        }
    }
}