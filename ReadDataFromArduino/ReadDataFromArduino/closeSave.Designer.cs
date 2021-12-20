
namespace ReadDataFromArduino
{
    partial class closeSave
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.csbtn1 = new System.Windows.Forms.Button();
            this.csbtn2 = new System.Windows.Forms.Button();
            this.cslb1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // csbtn1
            // 
            this.csbtn1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.csbtn1.Location = new System.Drawing.Point(185, 85);
            this.csbtn1.Name = "csbtn1";
            this.csbtn1.Size = new System.Drawing.Size(141, 52);
            this.csbtn1.TabIndex = 0;
            this.csbtn1.Text = "保存数据并退出程序";
            this.csbtn1.UseVisualStyleBackColor = true;
            // 
            // csbtn2
            // 
            this.csbtn2.Location = new System.Drawing.Point(365, 85);
            this.csbtn2.Name = "csbtn2";
            this.csbtn2.Size = new System.Drawing.Size(146, 52);
            this.csbtn2.TabIndex = 1;
            this.csbtn2.Text = "关闭软件";
            this.csbtn2.UseVisualStyleBackColor = true;
            // 
            // cslb1
            // 
            this.cslb1.AutoSize = true;
            this.cslb1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cslb1.Location = new System.Drawing.Point(22, 25);
            this.cslb1.Name = "cslb1";
            this.cslb1.Size = new System.Drawing.Size(325, 21);
            this.cslb1.TabIndex = 2;
            this.cslb1.Text = "你还没有保存最近一段时间的数据";
            // 
            // closeSave
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 162);
            this.Controls.Add(this.cslb1);
            this.Controls.Add(this.csbtn2);
            this.Controls.Add(this.csbtn1);
            this.Name = "closeSave";
            this.Text = "提示";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button csbtn1;
        private System.Windows.Forms.Button csbtn2;
        private System.Windows.Forms.Label cslb1;
    }
}