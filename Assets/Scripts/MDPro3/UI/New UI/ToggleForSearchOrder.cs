using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI 
{
    public class ToggleForSearchOrder : Toggle
    {
        public EditDeck.SortOrder sortOrder = EditDeck.SortOrder.ByType;

        public override void SwitchOn()
        {
            base.SwitchOn();
            Program.I().editDeck.manager.GetElement<Transform>("ButtonSort").GetChild(0).GetComponent<Image>().sprite = icon.sprite;
            Program.I().editDeck.sortOrder = sortOrder;
            Program.I().editDeck.OnClickSearch();
            transform.parent.parent.GetComponent<PopupSearchOrder>().Hide();
        }
        public override void SwitchOff()
        {
            SwitchOn();
        }
    }

}

