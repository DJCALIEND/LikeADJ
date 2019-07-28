using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public class CheckedComboBox : ComboBox
    {
        internal class Dropdown : Form
        {
            internal class CCBoxEventArgs : EventArgs
            {
                public bool AssignValues { get; set; }
                public EventArgs EventArgs { get; set; }
                public CCBoxEventArgs(EventArgs e, bool assignValues) : base()
                {
                    EventArgs = e;
                    AssignValues = assignValues;
                }
            }

            internal class CustomCheckedListBox : CheckedListBox
            {
                private int curSelIndex = -1;

                public CustomCheckedListBox() : base()
                {
                    SelectionMode = SelectionMode.One;
                    HorizontalScrollbar = true;                    
                }

                protected override void OnKeyDown(KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        ((CheckedComboBox.Dropdown) Parent).OnDeactivate(new CCBoxEventArgs(null, true));
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Escape)
                    {
                        ((CheckedComboBox.Dropdown) Parent).OnDeactivate(new CCBoxEventArgs(null, false));
                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Delete)
                    {
                        for (int i = 0; i < Items.Count; i++) {
                            SetItemChecked(i, e.Shift);
                        }
                        e.Handled = true;
                    }
                    base.OnKeyDown(e);
                }

                protected override void OnMouseMove(MouseEventArgs e)
                {
                    base.OnMouseMove(e);
                    int index = IndexFromPoint(e.Location);
                    if ((index >= 0) && (index != curSelIndex))
                    {
                        curSelIndex = index;
                        SetSelected(index, true);
                    }
                }
            } 

            private readonly CheckedComboBox ccbParent;
            private string oldStrValue = "";
            public bool ValueChanged
            {
                get
                {
                    string newStrValue = ccbParent.Text;
                    if ((oldStrValue.Length > 0) && (newStrValue.Length > 0))  return (oldStrValue.CompareTo(newStrValue) != 0);
                    else  return (oldStrValue.Length != newStrValue.Length);
                }
            }

            bool[] checkedStateArr;
            private bool dropdownClosed = true;
            public CustomCheckedListBox List { get; set; }

            public Dropdown(CheckedComboBox ccbParent)
            {
                this.ccbParent = ccbParent;
                InitializeComponent();
                ShowInTaskbar = false;
                List.ItemCheck += new ItemCheckEventHandler(this.Cclb_ItemCheck);
            }

            private void InitializeComponent()
            {
                List = new CustomCheckedListBox();
                SuspendLayout();

                List.BorderStyle = BorderStyle.None;
                List.Dock = DockStyle.Fill;
                List.FormattingEnabled = true;
                List.Location = new Point(0, 0);
                List.Name = "cclb";
                List.Size = new Size(47, 15);
                List.TabIndex = 0;

                AutoScaleDimensions = new SizeF(6F, 13F);
                AutoScaleMode = AutoScaleMode.Font;
                BackColor = SystemColors.Menu;
                ClientSize = new Size(47, 16);
                ControlBox = false;
                Controls.Add(this.List);
                ForeColor = SystemColors.ControlText;
                FormBorderStyle = FormBorderStyle.FixedToolWindow;
                MinimizeBox = false;
                Name = "ccbParent";
                StartPosition = FormStartPosition.Manual;
                ResumeLayout(false);
            }

            public string GetCheckedItemsStringValue()
            {
                StringBuilder sb = new StringBuilder("");
                for (int i = 0; i < List.CheckedItems.Count; i++)  sb.Append(List.GetItemText(List.CheckedItems[i])).Append(ccbParent.ValueSeparator);
                if (sb.Length > 0)  sb.Remove(sb.Length - ccbParent.ValueSeparator.Length, ccbParent.ValueSeparator.Length);

                return sb.ToString();
            }

            public void CloseDropdown(bool enactChanges)
            {
                if (dropdownClosed)  return;
            
                if (enactChanges)
                {
                    ccbParent.SelectedIndex = -1;                    
                    ccbParent.Text = GetCheckedItemsStringValue();
                }
                else { for (int i = 0; i < List.Items.Count; i++)  List.SetItemChecked(i, checkedStateArr[i]); }

                dropdownClosed = true;
                ccbParent.BeginInvoke(new MethodInvoker(() => this.Hide()));
                ccbParent.OnDropDownClosed(new CCBoxEventArgs(null, false));
            }

            protected override void OnActivated(EventArgs e)
            {
                base.OnActivated(e);
                dropdownClosed = false;
                oldStrValue = ccbParent.Text;
                checkedStateArr = new bool[List.Items.Count];
                for (int i = 0; i < List.Items.Count; i++)  checkedStateArr[i] = List.GetItemChecked(i);
            }

            protected override void OnDeactivate(EventArgs e)
            {
                base.OnDeactivate(e);
                if (e is CCBoxEventArgs ce) CloseDropdown(ce.AssignValues);
                else CloseDropdown(true);
            }

            private void Cclb_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                ccbParent.ItemCheck?.Invoke(sender, e);
            }
        } 

        private readonly System.ComponentModel.IContainer components = null;
        private readonly Dropdown dropdown;
        public string ValueSeparator { get; set; }

        public bool CheckOnClick
        {
            get { return dropdown.List.CheckOnClick; }
            set { dropdown.List.CheckOnClick = value; }
        }

        public new string DisplayMember
        {
            get { return dropdown.List.DisplayMember; }
            set { dropdown.List.DisplayMember = value; }
        }

        public new CheckedListBox.ObjectCollection Items
        {
            get { return dropdown.List.Items; }
        }

        public CheckedListBox.CheckedItemCollection CheckedItems
        {
            get { return dropdown.List.CheckedItems; }
        }
        
        public CheckedListBox.CheckedIndexCollection CheckedIndices
        {
            get { return dropdown.List.CheckedIndices; }
        }

        public bool ValueChanged
        {
            get { return dropdown.ValueChanged; }
        }

        public event ItemCheckEventHandler ItemCheck;
        
        public CheckedComboBox() : base()
        {
            DrawMode = DrawMode.OwnerDrawVariable;
            ValueSeparator = ",";
            DropDownHeight = 1;            
            DropDownStyle = ComboBoxStyle.DropDown;
            dropdown = new Dropdown(this);
            CheckOnClick = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))  components.Dispose();
            base.Dispose(disposing);
        }        

        protected override void OnDropDown(EventArgs e)
        {
            base.OnDropDown(e);
            DoDropDown();    
        }

        private void DoDropDown()
        {
            if (!dropdown.Visible)
            {
                Rectangle rect = RectangleToScreen(ClientRectangle);
                dropdown.Location = new Point(rect.X, rect.Y + Size.Height);
                int count = dropdown.List.Items.Count;
                if (count > MaxDropDownItems)  count = MaxDropDownItems;
                else if (count == 0)  count = 1;

                dropdown.Size = new Size(Size.Width, (dropdown.List.ItemHeight) * count + 2);
                dropdown.Show(this);
            }
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            if (e is Dropdown.CCBoxEventArgs) base.OnDropDownClosed(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)  OnDropDown(null);
            e.Handled = !e.Alt && !(e.KeyCode == Keys.Tab) && !((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) || (e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End));
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
            base.OnKeyPress(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            DroppedDown = false;
        }

        public bool GetItemChecked(int index)
        {
            if (index < 0 || index > Items.Count)  throw new ArgumentOutOfRangeException("index", "value out of range");
            else  return dropdown.List.GetItemChecked(index);
        }

        public void SetItemChecked(int index, bool isChecked)
        {
            if (index < 0 || index > Items.Count)  throw new ArgumentOutOfRangeException("index", "value out of range");
            else
            {
                dropdown.List.SetItemChecked(index, isChecked);
                Text = dropdown.GetCheckedItemsStringValue();
            }
        }

        public CheckState GetItemCheckState(int index)
        {
            if (index < 0 || index > Items.Count)  throw new ArgumentOutOfRangeException("index", "value out of range");
            else  return dropdown.List.GetItemCheckState(index);
        }

        public void SetItemCheckState(int index, CheckState state)
        {
            if (index < 0 || index > Items.Count)  throw new ArgumentOutOfRangeException("index", "value out of range");
            else
            {
                dropdown.List.SetItemCheckState(index, state);
                Text = dropdown.GetCheckedItemsStringValue();
            }
        }
    }   
}
