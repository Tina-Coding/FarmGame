using System.Windows.Controls;
using System.Windows.Input;

namespace SUP.Views.Game
{
    /// <summary>
    /// Interaction logic for GameBoardView.xaml
    /// </summary>
    public partial class GameBoardView : UserControl
    {
        public GameBoardView()
        {           
            InitializeComponent();
            Loaded += (s, e) => Keyboard.Focus(this);// Triggar igång KeyDown, vilktigt, annars händer det inget tydligen
        }
    }
}

