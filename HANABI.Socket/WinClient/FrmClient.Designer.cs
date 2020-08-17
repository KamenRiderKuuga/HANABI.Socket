namespace WinClient
{
    partial class FrmClient : HZH_Controls.Forms.FrmWithTitle
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new HZH_Controls.Controls.UCBtnExt();
            this.TextIP = new HZH_Controls.Controls.UCTextBoxEx();
            this.TextPort = new HZH_Controls.Controls.UCTextBoxEx();
            this.lbLog = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.White;
            this.btnConnect.BtnBackColor = System.Drawing.Color.White;
            this.btnConnect.BtnFont = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConnect.BtnForeColor = System.Drawing.Color.White;
            this.btnConnect.BtnText = "连接至服务器";
            this.btnConnect.ConerRadius = 5;
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.EnabledMouseEffect = false;
            this.btnConnect.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59)))));
            this.btnConnect.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.btnConnect.IsRadius = true;
            this.btnConnect.IsShowRect = true;
            this.btnConnect.IsShowTips = false;
            this.btnConnect.Location = new System.Drawing.Point(625, 90);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(0);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(58)))));
            this.btnConnect.RectWidth = 1;
            this.btnConnect.Size = new System.Drawing.Size(123, 39);
            this.btnConnect.TabIndex = 7;
            this.btnConnect.TabStop = false;
            this.btnConnect.TipsColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(30)))), ((int)(((byte)(99)))));
            this.btnConnect.TipsText = "";
            this.btnConnect.BtnClick += new System.EventHandler(this.btnConnect_BtnClick);
            // 
            // TextIP
            // 
            this.TextIP.BackColor = System.Drawing.Color.Transparent;
            this.TextIP.ConerRadius = 5;
            this.TextIP.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextIP.DecLength = 2;
            this.TextIP.FillColor = System.Drawing.Color.Empty;
            this.TextIP.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59)))));
            this.TextIP.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TextIP.InputText = "";
            this.TextIP.InputType = HZH_Controls.TextInputType.Regex;
            this.TextIP.IsFocusColor = true;
            this.TextIP.IsRadius = true;
            this.TextIP.IsShowClearBtn = true;
            this.TextIP.IsShowKeyboard = false;
            this.TextIP.IsShowRect = true;
            this.TextIP.IsShowSearchBtn = false;
            this.TextIP.KeyBoardType = HZH_Controls.Controls.KeyBoardType.UCKeyBorderAll_EN;
            this.TextIP.Location = new System.Drawing.Point(67, 90);
            this.TextIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextIP.MaxValue = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.TextIP.MinValue = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.TextIP.Name = "TextIP";
            this.TextIP.Padding = new System.Windows.Forms.Padding(5);
            this.TextIP.PasswordChar = '\0';
            this.TextIP.PromptColor = System.Drawing.Color.Gray;
            this.TextIP.PromptFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TextIP.PromptText = "服务器IP地址";
            this.TextIP.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.TextIP.RectWidth = 1;
            this.TextIP.RegexPattern = "[0-9\\.]";
            this.TextIP.Size = new System.Drawing.Size(327, 39);
            this.TextIP.TabIndex = 9;
            // 
            // TextPort
            // 
            this.TextPort.BackColor = System.Drawing.Color.Transparent;
            this.TextPort.ConerRadius = 5;
            this.TextPort.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextPort.DecLength = 2;
            this.TextPort.FillColor = System.Drawing.Color.Empty;
            this.TextPort.FocusBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(77)))), ((int)(((byte)(59)))));
            this.TextPort.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.TextPort.InputText = "100";
            this.TextPort.InputType = HZH_Controls.TextInputType.UnsignNumber;
            this.TextPort.IsFocusColor = true;
            this.TextPort.IsRadius = true;
            this.TextPort.IsShowClearBtn = false;
            this.TextPort.IsShowKeyboard = false;
            this.TextPort.IsShowRect = true;
            this.TextPort.IsShowSearchBtn = false;
            this.TextPort.KeyBoardType = HZH_Controls.Controls.KeyBoardType.UCKeyBorderAll_EN;
            this.TextPort.Location = new System.Drawing.Point(434, 90);
            this.TextPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextPort.MaxValue = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.TextPort.MinValue = new decimal(new int[] {
            1000000,
            0,
            0,
            -2147483648});
            this.TextPort.Name = "TextPort";
            this.TextPort.Padding = new System.Windows.Forms.Padding(5);
            this.TextPort.PasswordChar = '\0';
            this.TextPort.PromptColor = System.Drawing.Color.Gray;
            this.TextPort.PromptFont = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TextPort.PromptText = "端口号";
            this.TextPort.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.TextPort.RectWidth = 1;
            this.TextPort.RegexPattern = "";
            this.TextPort.Size = new System.Drawing.Size(93, 39);
            this.TextPort.TabIndex = 10;
            // 
            // lbLog
            // 
            this.lbLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lbLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lbLog.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.lbLog.FormattingEnabled = true;
            this.lbLog.HorizontalScrollbar = true;
            this.lbLog.ItemHeight = 17;
            this.lbLog.Location = new System.Drawing.Point(0, 157);
            this.lbLog.Name = "lbLog";
            this.lbLog.ScrollAlwaysVisible = true;
            this.lbLog.Size = new System.Drawing.Size(800, 293);
            this.lbLog.TabIndex = 11;
            // 
            // FrmClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lbLog);
            this.Controls.Add(this.TextPort);
            this.Controls.Add(this.TextIP);
            this.Controls.Add(this.btnConnect);
            this.IsShowCloseBtn = true;
            this.Name = "FrmClient";
            this.ShowInTaskbar = true;
            this.Text = "FrmClient";
            this.Title = "客户端";
            this.Controls.SetChildIndex(this.btnConnect, 0);
            this.Controls.SetChildIndex(this.TextIP, 0);
            this.Controls.SetChildIndex(this.TextPort, 0);
            this.Controls.SetChildIndex(this.lbLog, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private HZH_Controls.Controls.UCBtnExt btnConnect;
        private HZH_Controls.Controls.UCTextBoxEx TextIP;
        private HZH_Controls.Controls.UCTextBoxEx TextPort;
        private System.Windows.Forms.ListBox lbLog;
    }
}

