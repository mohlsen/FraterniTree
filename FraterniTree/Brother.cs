﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TreeDisplay;

namespace FraterniTree
{
    /// <summary>
    /// Represents a Fraternity Brother in a Fraternity Family Tree.
    /// </summary>
    public class Brother
    {
        public string  m_Last;
        public string  m_First;
        public string  m_IniMonth;
        public int     m_IniYear;
        public Label   m_Label = new Label();
        public Action<Brother> m_Callback = null;
        private bool areChildrenHidden = false;
        private Point lastPos;

        /// <summary>
        /// Reference to a Node object which represents all relational data and methods.
        /// </summary>
        private Node m_NodeRef;


        #region Constructors

        /// <summary>
        /// Disallow non-parameterized creation of object
        /// </summary>
        private Brother()
        {

        }

        /// <summary>
        /// Allow a Brother to be created only by specifying the following parameters
        /// </summary>
        /// <param name="Last">Last Name of the Brother</param>
        /// <param name="First">First Name of the Brother</param>
        /// <param name="Month">Initiation Term of the Brother</param>
        /// <param name="Year">Initiation Year of the Brother</param>
        public Brother(string Last, string First, string Month, int Year)
        {
            // Initialize Brother object
            m_Last              = Last;
            m_First             = First;
            m_IniMonth          = Month;
            m_IniYear           = Year;            

            // Initialize the label
            m_Label.Text        = m_First + " " + m_Last;
            m_Label.TextAlign   = System.Drawing.ContentAlignment.MiddleCenter;
            m_Label.Padding     = new Padding(4);
            m_Label.AutoSize    = true;
            m_Label.BorderStyle = BorderStyle.FixedSingle;
            m_Label.MouseClick += m_Label_MouseClick;
            m_Label.MouseDown  += m_Label_MouseDown;
            m_Label.MouseMove  += m_Label_MouseMove;
            m_Label.MouseUp    += m_Label_MouseUp;
            m_Label.Paint      += m_Label_Paint;
            m_Label.LocationChanged += m_Label_LocationChanged;
            m_Label.ParentChanged += m_Label_ParentChanged;

            m_NodeRef = new Node();
            m_NodeRef.SetWidth(m_Label.Width);
            m_NodeRef.SetHeight(m_Label.Height);
            m_NodeRef.SetText(m_Label.Text);
            m_NodeRef.SetUserData(this);
            m_NodeRef.SetCallback(ApplyNodeLocationsToLabel);
        }

        #endregion

        public string GetFullName()
        {
            return m_First + " " + m_Last;
        }

        public Node GetNodeRef()
        {
            return m_NodeRef;
        }

        public Brother FindBrotherByName(string fullName)
        {
            Brother found = null;

            if (this.GetFullName() == fullName)
            {
                found = this;
            }

            if (this.GetNodeRef().FirstChild() != null && found == null)
            {
                found = ((Brother)(this.GetNodeRef().FirstChild().GetUserData())).FindBrotherByName(fullName);
            }

            if (this.GetNodeRef().RightSibling() != null && found == null)
            {
                found = ((Brother)(this.GetNodeRef().RightSibling().GetUserData())).FindBrotherByName(fullName);
            }

            return found;
        }

        #region GUI Label Methods

        private void ApplyNodeLocationsToLabel()
        {
            m_Label.Location = new Point(m_NodeRef.GetXCoord(), m_NodeRef.GetYCoord());
        }

        private void RecursiveLabelMove(Brother b, int dx, int dy)
        {
            if (b.m_Label.Parent != null)
            {
                b.m_Label.Location = new Point(b.m_Label.Left + dx, b.m_Label.Top + dy);
                if (b.GetNodeRef().HasRightSibling())
                {
                    RecursiveLabelMove((Brother)(b.GetNodeRef().RightSibling().GetUserData()), dx, dy);
                }
                if (b.GetNodeRef().HasChild())
                {
                    RecursiveLabelMove((Brother)(b.GetNodeRef().FirstChild().GetUserData()), dx, dy);
                }
            }
        }

        private void RecursiveLabelCapture(Brother b, MouseEventArgs e)
        {
            if (b.m_Label.Parent != null)
            {
                b.lastPos = e.Location;
                b.m_Label.BringToFront();
                if (b.GetNodeRef().HasRightSibling())
                {
                    RecursiveLabelCapture((Brother)(b.GetNodeRef().RightSibling().GetUserData()), e);
                }
                if (b.GetNodeRef().HasChild())
                {
                    RecursiveLabelCapture((Brother)(b.GetNodeRef().FirstChild().GetUserData()), e);
                }
            }
        }

        private void RecursiveLabelVisibleToggle(Brother b)
        {
            if (b.m_Label.Parent != null)
            {
                b.m_Label.Visible = !b.m_Label.Visible;
            }
            if (!b.areChildrenHidden)
            {
                for (int i = b.GetNodeRef().GetNumberOfChildren() - 1; i >= 0; i--)
                {
                    RecursiveLabelVisibleToggle((Brother)(b.GetNodeRef()[i].GetUserData()));
                }
            }
        }

        private void m_Label_ParentChanged(object sender, EventArgs e)
        {
            // Does nothing
        }

        private void m_Label_LocationChanged(object sender, EventArgs e)
        {
            // Does nothing
        }

        private void m_Label_Paint(object sender, PaintEventArgs e)
        {
            m_Label.Parent.Invalidate();
        }

        private void m_Label_MouseUp(object sender, MouseEventArgs e)
        {
            m_Label.Capture = false;
            m_Label.Parent.Invalidate();
        }

        private void m_Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.X - lastPos.X;
                int dy = e.Y - lastPos.Y;
                m_Label.Location = new Point(m_Label.Left + dx, m_Label.Top + dy);
                if (this.GetNodeRef().HasChild())
                {
                    RecursiveLabelMove((Brother)this.GetNodeRef().FirstChild().GetUserData(), dx, dy);
                }
            }
        }

        private void m_Label_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastPos = e.Location;
                m_Label.BringToFront();
                m_Label.Capture = true;
                if (this.GetNodeRef().HasChild())
                {
                    RecursiveLabelCapture((Brother)this.GetNodeRef().FirstChild().GetUserData(), e);
                }
            }
        }

        void m_Label_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_Label.BackColor = System.Drawing.Color.SpringGreen;
                if (m_Callback != null)
                {
                    m_Callback(this);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (this.GetNodeRef().FirstChild() == null)
                {
                    return;
                }
                Brother firstChild = ((Brother)(this.GetNodeRef().FirstChild().GetUserData()));
                if (firstChild.m_Label.Parent != null)
                {
                    if (firstChild.m_Label.Visible)
                    {
                        this.m_Label.Text += "*";
                        areChildrenHidden = true;
                    }
                    else
                    {
                        this.m_Label.Text = this.GetFullName();
                        areChildrenHidden = false;
                    }
                    for (int i = this.GetNodeRef().GetNumberOfChildren() - 1; i >= 0; i--)
                    {
                        RecursiveLabelVisibleToggle((Brother)(this.GetNodeRef()[i].GetUserData()));
                    }
                }
            }
        }

        #endregion

    }
}