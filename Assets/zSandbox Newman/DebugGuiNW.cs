using UnityEngine;
using System.Collections;

public class DebugGuiNW : MonoBehaviour {
	
	private int    	StartX			= 260;
	private int    	StartY			= 200;
	private int    	WidthX			= 200;
	private int    	WidthY			= 30;
	private int    	ButtonOffsetY	= 32;
	private int		SideButtonOffsetX = 30;
	
	private bool 	isGuiOn			= true;
	
	private int SelectedIndex = 0;	// Indicates what key is currently selected by Keyboard
	
	static private int NumButtons = 4;
	private Color[] ButtonColors = new Color[NumButtons];
	
	private ArrayList ButtonsList = new ArrayList();
	
	private float accel = 0;
	private float brake = -200;
	private float maxSpeed = 2000;
	private float sens = 20;
	
	// Use this for initialization
	void Start () {
		// Initializes GUI buttons to white, except for the selectedIndex
		//ButtonColors[SelectedIndex] = Color.yellow;
		//for(int i = 1; i < ButtonColors.Length; i++)
		//	ButtonColors[i] = Color.white;
		      
		//ButtonsNW butt1 = new ButtonsNW(StartX, StartY, WidthX, WidthY, "Acceleration: ", Color.yellow);
		//ButtonsList.Add(butt1);
		//butt1.Display();
		
		//ButtonsList.Add(new ButtonsNW(StartX, StartY, WidthX, WidthY, "Acceleration: ", Color.yellow));
		//ButtonsList.Add(new ButtonsNW(StartX, StartY + ButtonOffsetY, WidthX, WidthY, "Max Speed: ", Color.white));
		//ButtonsList.Add(new ButtonsNW(StartX, StartY + ButtonOffsetY * 2, WidthX, WidthY, "Sensitivity: ", Color.white));		
		//ButtonsList.Add(new ButtonsNW(StartX, StartY + ButtonOffsetY * 3, WidthX, WidthY, "Brake: ", Color.white));
		
		AddButton (StartX, StartY, WidthX, WidthY, "Acceleration: ", Color.white);
		AddButton (StartX, StartY + ButtonOffsetY, WidthX, WidthY, "Max Speed: ", Color.white);
		AddButton (StartX, StartY + ButtonOffsetY * 2, WidthX, WidthY, "Sensitivity: ", Color.white);
		AddButton (StartX, StartY + ButtonOffsetY * 3, WidthX, WidthY, "Brake: ", Color.white);
		
		
		((ButtonsNW)ButtonsList[0]).ButtonSelected();
	}
	
	
	// Update is called once per frame
	void Update () {
		KeyboardMenuSelection();
		accel++;
		maxSpeed--;
		sens -= .001f;
		
		if(brake < 20)
			brake++;
		else
			brake = -20;
	}
	
	void OnGUI(){
		if(!isGuiOn){
			return;	
		}
		
		// Title
		GuiUtilsNW.GUIStereoButton (StartX, StartY - 60, WidthX, WidthY, "Name of controller here", Color.cyan);
		
		// Creates Boxes
		// When adding new buttons, make sure to increase 'NumButtons' variable at top
		/*GuiUtilsNW.GUIStereoButton (StartX, StartY, WidthX, WidthY, "Acceleration: ", ButtonColors[0]);
		GuiUtilsNW.GUIStereoButton (StartX, StartY + ButtonOffsetY, WidthX, WidthY, "Max Speed: ", ButtonColors[1]);
		GuiUtilsNW.GUIStereoButton (StartX, StartY + ButtonOffsetY * 2, WidthX, WidthY, "Sensitivity: ", ButtonColors[2]);
		GuiUtilsNW.GUIStereoButton (StartX, StartY + ButtonOffsetY * 3, WidthX, WidthY, "Brake: ", ButtonColors[3]);
		*/
		
		// Need to change this for ease of use
		for(int i = 0; i < ButtonsList.Count; i++){
			
			float holder;
			switch(i)
			{
			case 0:
				holder = accel;
				break;
			case 1:
				holder = maxSpeed;
				break;
			case 2:
				holder = sens;
				break;
			case 3:
				holder = brake;
				break;
			default:
				holder = 999999999999.999f;
				break;
			}
			((ButtonsNW)(ButtonsList[i])).DynamicDisplay(holder);
			
			//((ButtonsNW)(ButtonsList[i])).UpdateText((float)DynamicInfo[i]);
			//((ButtonsNW)(ButtonsList[i])).Display();
				
			//if(i == 0)
			//	((ButtonsNW)(ButtonsList[i])).UpdateText(accel);
			//else if (i = 1)
			
			//((ButtonsNW)(ButtonsList[i])).Display();
			//((ButtonsNW)(ButtonsList[i])).UpdatedDisplay();
		}
		
		// Buttons appear next to currently selected button
		GuiUtilsNW.GUIStereoButton (StartX - 32, StartY + ButtonOffsetY * SelectedIndex, 30, 30, "Z", Color.green);
		GuiUtilsNW.GUIStereoButton (StartX + 202, StartY + ButtonOffsetY * SelectedIndex, 30, 30, "X", Color.green);
		
		
		
	}
	
	// Determines what each button does when pressed
	void KeyboardMenuSelection() {
		if(Input.GetKeyDown(KeyCode.Return)){
			if (SelectedIndex == 0){
				// Resume Game	
			}
			if(SelectedIndex == 1){
				// Quit game	
			}	
		}
		
		// Moves between buttons with arrows keys
		if(Input.GetKeyDown(KeyCode.DownArrow) && SelectedIndex < ButtonsList.Count - 1){
			Color col = Color.white;
			((ButtonsNW)ButtonsList[SelectedIndex]).ButtonDeselected();
			//((ButtonsNW)ButtonsList[SelectedIndex]).ChangeColor(col);
			SelectedIndex++;
			// ChangeButtonColor(SelectedIndex - 1);
			//((ButtonsNW)ButtonsList[SelectedIndex]).ChangeColor(Color.yellow);
			((ButtonsNW)ButtonsList[SelectedIndex]).ButtonSelected();
		}
		if(Input.GetKeyDown(KeyCode.UpArrow) && SelectedIndex > 0){
			((ButtonsNW)ButtonsList[SelectedIndex]).ButtonDeselected();
			//((ButtonsNW)ButtonsList[SelectedIndex]).ChangeColor(Color.white);
			SelectedIndex--;
			//((ButtonsNW)ButtonsList[SelectedIndex]).ChangeColor(Color.yellow);
			// ChangeButtonColor(SelectedIndex + 1);
			((ButtonsNW)ButtonsList[SelectedIndex]).ButtonSelected();
		}
	}
	
	void AddButton(int X, int Y, int wX, int wY, string text, Color color){
		ButtonsList.Add (new ButtonsNW(X, Y, wX, wY, text, color));
	}
		
	void AddBox(){
		
	}	
	
	// Changes the color of the GUI button if it is currently selected by the keyboard
	/*void ChangeButtonColor(int prevIndex){
		ButtonColors[prevIndex] = Color.white;
		ButtonColors[SelectedIndex] = Color.yellow;
	}*/

}
