using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Sintef.Scoop.Utilities.GUI
{
	/// <summary>
	/// Interaction logic for StringInputDialog.xaml
	/// </summary>
	public partial class StringInputDialog : Window
	{
		/// <summary>
		/// The question to ask the user
		/// </summary>
		public string Question
		{
			get => _question.Content.ToString();
			set
			{
				_question.Content = value;
			}
		}

		/// <summary>
		/// The input text from the user. May be initialized with some default text before opening the dialog.
		/// </summary>
		public string UserInput
		{
			get => _userInput.Text;
			set
			{
				_userInput.Text = value;
			}
		}

		/// <summary>
		/// Initializes the dialog
		/// </summary>
		public StringInputDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Utility function for easy use of the dialog. Initializes a dialog with the input 
		/// data, and opens it modally.
		/// </summary>
		/// <returns>Returns the input from the user, or null if the dialog was cancelled.</returns>
		public static string GetInputFromUser(string question, string defaultAnswer)
		{
			StringInputDialog dia = new StringInputDialog();
			dia.Question = question;
			dia.UserInput = defaultAnswer;

			var result = dia.ShowDialog();
			if (result == null || !(result.Value))
				return null;
			else
				return dia.UserInput;
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
