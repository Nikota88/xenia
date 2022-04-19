using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace shader_playground
{
    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    public class NewBaseType
    {
        private const int SB_VERT = 1;
        private const uint SB_THUMBPOSITION = 4;
        private const uint WM_VSCROLL = 0x115;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox wordsTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox sourceCodeTextBox;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.TextBox compilerUcodeTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox compilerTranslatedTextBox;
        private System.Windows.Forms.ComboBox translationComboBox;
        private System.Windows.Forms.ComboBox vertexShaderComboBox;
        string compilerPath_ = @"..\..\..\..\..\build\bin\Windows\Debug\xenia-gpu-shader-compiler.exe";

        FileSystemWatcher compilerWatcher_;
        bool pendingTimer_ = false;

        Dictionary<IntPtr, bool> scrollPreserve_ = new Dictionary<IntPtr, bool>();
        Dictionary<IntPtr, int> scrollPositions_ = new Dictionary<IntPtr, int>();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetScrollInfo(IntPtr hwnd, int bar, ref ScrollInfo scrollInfo);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetScrollInfo(IntPtr hwnd, int bar, ref ScrollInfo scrollInfo, int redraw);

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

        uint[] ExtractAndDumpWords(string shaderType, byte[] shaderCode)
        {
            if (shaderCode == null || shaderCode.Length == 0)
            {
                UpdateTextBox(wordsTextBox, "", false);
                return null;
            }

            // Find shader code.
            int byteOffset = (shaderCode[4] << 24) | (shaderCode[5] << 16) |
                             (shaderCode[6] << 8) | (shaderCode[7] << 0);
            int wordOffset = byteOffset / 4;

            uint[] shaderDwords = new uint[(shaderCode.Length - wordOffset) / sizeof(uint)];
            Buffer.BlockCopy(shaderCode, wordOffset * 4, shaderDwords, 0, shaderCode.Length - wordOffset * 4);

            var sb = new StringBuilder();
            sb.Append("const uint32_t shader_dwords[] = {");
            for (int i = 0; i < shaderDwords.Length; ++i)
            {
                sb.AppendFormat("0x{0:X8}, ", SwapByte(shaderDwords[i]));
            }
            sb.Append("};" + Environment.NewLine);
            sb.Append("shader_type = ShaderType::" + (shaderType == "vs" ? "kVertex" : "kPixel") + ";" + Environment.NewLine);
            UpdateTextBox(wordsTextBox, sb.ToString(), true);
            wordsTextBox.SelectAll();

            return shaderDwords;
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }


        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wordsTextBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.vertexShaderComboBox = new System.Windows.Forms.ComboBox();
            this.compilerTranslatedTextBox = new System.Windows.Forms.TextBox();
            this.sourceCodeTextBox = new System.Windows.Forms.TextBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.compilerUcodeTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.translationComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // wordsTextBox
            // 
            this.wordsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.wordsTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wordsTextBox.Location = new System.Drawing.Point(12, 657);
            this.wordsTextBox.Multiline = true;
            this.wordsTextBox.Name = "wordsTextBox";
            this.wordsTextBox.ReadOnly = true;
            this.wordsTextBox.Size = new System.Drawing.Size(1631, 137);
            this.wordsTextBox.TabIndex = 11;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.vertexShaderComboBox, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.compilerTranslatedTextBox, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.sourceCodeTextBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.outputTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.compilerUcodeTextBox, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.translationComboBox, 3, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1631, 639);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // vertexShaderComboBox
            // 
            this.vertexShaderComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.vertexShaderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vertexShaderComboBox.FormattingEnabled = true;
            this.vertexShaderComboBox.Items.AddRange(new object[] {
            "VS to VS",
            "VS to line DS with control point indices",
            "VS to line DS with patch index",
            "VS to triangle DS with control point indices",
            "VS to triangle DS with patch index",
            "VS to quad DS with control point indices",
            "VS to quad DS with patch index"});
            this.vertexShaderComboBox.Location = new System.Drawing.Point(1224, 24);
            this.vertexShaderComboBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.vertexShaderComboBox.Name = "vertexShaderComboBox";
            this.vertexShaderComboBox.Size = new System.Drawing.Size(404, 21);
            this.vertexShaderComboBox.TabIndex = 7;
            // 
            // compilerTranslatedTextBox
            // 
            this.compilerTranslatedTextBox.AcceptsReturn = true;
            this.compilerTranslatedTextBox.AcceptsTab = true;
            this.compilerTranslatedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.compilerTranslatedTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compilerTranslatedTextBox.Location = new System.Drawing.Point(1224, 48);
            this.compilerTranslatedTextBox.Multiline = true;
            this.compilerTranslatedTextBox.Name = "compilerTranslatedTextBox";
            this.compilerTranslatedTextBox.ReadOnly = true;
            this.compilerTranslatedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.compilerTranslatedTextBox.Size = new System.Drawing.Size(404, 588);
            this.compilerTranslatedTextBox.TabIndex = 10;
            // 
            // sourceCodeTextBox
            // 
            this.sourceCodeTextBox.AcceptsReturn = true;
            this.sourceCodeTextBox.AcceptsTab = true;
            this.sourceCodeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceCodeTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourceCodeTextBox.Location = new System.Drawing.Point(3, 24);
            this.sourceCodeTextBox.Multiline = true;
            this.sourceCodeTextBox.Name = "sourceCodeTextBox";
            this.tableLayoutPanel1.SetRowSpan(this.sourceCodeTextBox, 2);
            this.sourceCodeTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.sourceCodeTextBox.Size = new System.Drawing.Size(401, 612);
            this.sourceCodeTextBox.TabIndex = 5;
            // 
            // outputTextBox
            // 
            this.outputTextBox.AcceptsReturn = true;
            this.outputTextBox.AcceptsTab = true;
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputTextBox.Location = new System.Drawing.Point(410, 24);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.tableLayoutPanel1.SetRowSpan(this.outputTextBox, 2);
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(401, 612);
            this.outputTextBox.TabIndex = 8;
            // 
            // compilerUcodeTextBox
            // 
            this.compilerUcodeTextBox.AcceptsReturn = true;
            this.compilerUcodeTextBox.AcceptsTab = true;
            this.compilerUcodeTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.compilerUcodeTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compilerUcodeTextBox.Location = new System.Drawing.Point(817, 24);
            this.compilerUcodeTextBox.Multiline = true;
            this.compilerUcodeTextBox.Name = "compilerUcodeTextBox";
            this.compilerUcodeTextBox.ReadOnly = true;
            this.tableLayoutPanel1.SetRowSpan(this.compilerUcodeTextBox, 2);
            this.compilerUcodeTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.compilerUcodeTextBox.Size = new System.Drawing.Size(401, 612);
            this.compilerUcodeTextBox.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input Assembly";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(410, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "XNA Compiler Output";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(817, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(191, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "xenia-gpu-shader-compiler Disassembly";
            // 
            // translationComboBox
            // 
            this.translationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
            this.translationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.translationComboBox.FormattingEnabled = true;
            this.translationComboBox.Items.AddRange(new object[] {
            "DXBC (RTV/DSV RB)",
            "DXBC (ROV RB)",
            "SPIR-V"});
            this.translationComboBox.Location = new System.Drawing.Point(1224, 0);
            this.translationComboBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.translationComboBox.Name = "translationComboBox";
            this.translationComboBox.Size = new System.Drawing.Size(404, 21);
            this.translationComboBox.TabIndex = 6;
            // 
            // Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1655, 806);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.wordsTextBox);
            this.Name = "Editor";
            this.Text = "Shader Playground";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        bool MemCmp(byte[] a1, byte[] b1)
        {
            if (a1 == null || b1 == null)
            {
                return false;
            }
            int length = a1.Length;
            if (b1.Length != length)
            {
                return false;
            }
            while (length > 0)
            {
                length--;
                if (a1[length] != b1[length])
                {
                    return false;
                }
            }
            return true;
        }

        void Process(string shaderSourceCode)
        {
            if (shaderSourceCode.IndexOf("xvs_3_0") != -1 || shaderSourceCode.IndexOf("xps_3_0") != -1)
            {
                shaderSourceCode += "\ncnop";
                shaderSourceCode += "\ncnop";
            }
            var preprocessorDefines = new CompilerMacro[2];
            preprocessorDefines[0].Name = "XBOX";
            preprocessorDefines[1].Name = "XBOX360";
            var includeHandler = new NopIncludeHandler();
            var options = CompilerOptions.None;
            var compiledShader = ShaderCompiler.AssembleFromSource(
                shaderSourceCode, preprocessorDefines, includeHandler, options,
                Microsoft.Xna.Framework.TargetPlatform.Xbox360);

            var disassembledSourceCode = compiledShader.ErrorsAndWarnings;
            disassembledSourceCode = disassembledSourceCode.Replace("\n", Environment.NewLine);
            if (disassembledSourceCode.IndexOf("// PDB hint 00000000-00000000-00000000") == -1)
            {
                UpdateTextBox(outputTextBox, disassembledSourceCode, false);
                UpdateTextBox(compilerUcodeTextBox, "", false);
                UpdateTextBox(wordsTextBox, "", false);
                return;
            }
            var prefix = disassembledSourceCode.Substring(
                0, disassembledSourceCode.IndexOf(
                       ':', disassembledSourceCode.IndexOf(':') + 1));
            disassembledSourceCode =
                disassembledSourceCode.Replace(prefix + ": ", "");
            disassembledSourceCode = disassembledSourceCode.Replace(
                "// PDB hint 00000000-00000000-00000000" + Environment.NewLine, "");
            var firstLine = disassembledSourceCode.IndexOf("//");
            var warnings = "// " +
                           disassembledSourceCode.Substring(0, firstLine)
                               .Replace(Environment.NewLine, Environment.NewLine + "// ");
            disassembledSourceCode =
                warnings + disassembledSourceCode.Substring(firstLine + 3);
            disassembledSourceCode = disassembledSourceCode.Trim();
            UpdateTextBox(outputTextBox, disassembledSourceCode, true);

            string shaderType =
                shaderSourceCode.IndexOf("vs_") == -1 ? "ps" : "vs";
            var ucodeWords = ExtractAndDumpWords(shaderType, compiledShader.GetShaderCode());
            if (ucodeWords != null)
            {
                TryCompiler(shaderType, ucodeWords);
            }
            else
            {
                UpdateTextBox(compilerUcodeTextBox, "", false);
            }

            if (compilerUcodeTextBox.Text.Length > 0)
            {
                var sourcePrefix = disassembledSourceCode.Substring(0, disassembledSourceCode.IndexOf("/*"));
                TryRoundTrip(sourcePrefix, compilerUcodeTextBox.Text, compiledShader.GetShaderCode());
            }
        }

        uint SwapByte(uint x)
        {
            return ((x & 0x000000ff) << 24) +
                   ((x & 0x0000ff00) << 8) +
                   ((x & 0x00ff0000) >> 8) +
                   ((x & 0xff000000) >> 24);
        }

        void TryCompiler(string shaderType, uint[] ucodeWords)
        {
            string ucodePath = Path.Combine(Path.GetTempPath(), "shader_playground_ucode.bin." + shaderType);
            string ucodeDisasmPath = Path.Combine(Path.GetTempPath(), "shader_playground_disasm.ucode.txt");
            string translatedDisasmPath = Path.Combine(Path.GetTempPath(), "shader_playground_disasm.translated.txt");
            if (File.Exists(ucodePath))
            {
                File.Delete(ucodePath);
            }
            if (File.Exists(ucodeDisasmPath))
            {
                File.Delete(ucodeDisasmPath);
            }
            if (File.Exists(translatedDisasmPath))
            {
                File.Delete(translatedDisasmPath);
            }

            byte[] ucodeBytes = new byte[ucodeWords.Length * 4];
            Buffer.BlockCopy(ucodeWords, 0, ucodeBytes, 0, ucodeWords.Length * 4);
            File.WriteAllBytes(ucodePath, ucodeBytes);

            if (!File.Exists(compilerPath_))
            {
                UpdateTextBox(compilerUcodeTextBox, "Compiler not found: " + compilerPath_, false);
                return;
            }

            var startInfo = new ProcessStartInfo(compilerPath_);
            startInfo.Arguments = string.Join(" ", new string[]{
        "--shader_input=" + ucodePath,
        "--shader_input_type=" + shaderType,
        "--shader_output=" + ucodeDisasmPath,
        "--shader_output_type=ucode",
      });
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            try
            {
                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
                string disasmText = File.ReadAllText(ucodeDisasmPath);
                UpdateTextBox(compilerUcodeTextBox, disasmText.Replace("\n", Environment.NewLine), true);
            }
            catch
            {
                UpdateTextBox(compilerUcodeTextBox, "COMPILER FAILURE", false);
            }

            string outputType = "ucode";
            switch (translationComboBox.SelectedIndex)
            {
                case 0:
                case 1:
                    outputType = "dxbctext";
                    break;
                case 2:
                    outputType = "spirvtext";
                    break;
            }

            string vertexShaderType = "vertex";
            switch (vertexShaderComboBox.SelectedIndex)
            {
                case 1:
                    vertexShaderType = "linedomaincp";
                    break;
                case 2:
                    vertexShaderType = "linedomainpatch";
                    break;
                case 3:
                    vertexShaderType = "triangledomaincp";
                    break;
                case 4:
                    vertexShaderType = "triangledomainpatch";
                    break;
                case 5:
                    vertexShaderType = "quaddomaincp";
                    break;
                case 6:
                    vertexShaderType = "quaddomainpatch";
                    break;
            }

            List<string> startArguments = new List<string>{
        "--shader_input=" + ucodePath,
        "--shader_input_type=" + shaderType,
        "--shader_output=" + translatedDisasmPath,
        "--shader_output_type=" + outputType,
        "--vertex_shader_output_type=" + vertexShaderType,
        "--dxbc_source_map=true",
      };
            if (translationComboBox.SelectedIndex == 1)
            {
                startArguments.Add("--shader_output_dxbc_rov=true");
            }

            startInfo = new ProcessStartInfo(compilerPath_);
            startInfo.Arguments = string.Join(" ", startArguments.ToArray());
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            try
            {
                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
                string disasmText = File.ReadAllText(translatedDisasmPath);
                UpdateTextBox(compilerTranslatedTextBox, disasmText.Replace("\n", Environment.NewLine), true);
            }
            catch
            {
                UpdateTextBox(compilerTranslatedTextBox, "COMPILER FAILURE", false);
            }
        }

        void TryRoundTrip(string sourcePrefix, string compilerSource, byte[] expectedBytes)
        {
            var shaderSourceCode = sourcePrefix + compilerSource;
            var preprocessorDefines = new CompilerMacro[2];
            preprocessorDefines[0].Name = "XBOX";
            preprocessorDefines[1].Name = "XBOX360";
            var includeHandler = new NopIncludeHandler();
            var options = CompilerOptions.None;
            var compiledShader = ShaderCompiler.AssembleFromSource(
                shaderSourceCode, preprocessorDefines, includeHandler, options,
                Microsoft.Xna.Framework.TargetPlatform.Xbox360);
            var compiledBytes = compiledShader.GetShaderCode();
            if (compiledBytes == null ||
                compiledBytes.Length != expectedBytes.Length ||
                !MemCmp(compiledBytes, expectedBytes))
            {
                compilerUcodeTextBox.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                compilerUcodeTextBox.BackColor = System.Drawing.SystemColors.Control;
            }
        }

        void UpdateScrollStates()
        {
            foreach (var handle in scrollPreserve_.Keys)
            {
                if (scrollPreserve_[handle])
                {
                    var scrollInfo = new ScrollInfo();
                    scrollInfo.cbSize = Marshal.SizeOf(scrollInfo);
                    scrollInfo.fMask = (uint)ScrollInfoMask.SIF_TRACKPOS;
                    bool hasScrollInfo = GetScrollInfo(handle, SB_VERT, ref scrollInfo);
                    scrollPositions_[handle] = scrollInfo.nTrackPos;
                }
            }
        }
        void UpdateTextBox(TextBox textBox, string value, bool preserveScroll)
        {
            scrollPreserve_[textBox.Handle] = preserveScroll;

            textBox.Text = value;

            int previousScroll;
            if (!scrollPositions_.TryGetValue(textBox.Handle, out previousScroll))
            {
                previousScroll = 0;
            }
            var scrollInfo = new ScrollInfo();
            scrollInfo.cbSize = Marshal.SizeOf(scrollInfo);
            scrollInfo.fMask = (uint)ScrollInfoMask.SIF_TRACKPOS;
            scrollInfo.nTrackPos = previousScroll;
            SetScrollInfo(textBox.Handle, SB_VERT, ref scrollInfo, 1);

            var ptrWparam = new IntPtr(SB_THUMBPOSITION | previousScroll << 16);
            SendMessage(textBox.Handle, WM_VSCROLL, ptrWparam, IntPtr.Zero);
        }
    }

    [DebuggerDisplayAttribute]
    public class NewBaseType
    {
    }

    [DebuggerDisplayAttribute]
    public class NewBaseType
    {
    }

    [DebuggerDisplayAttribute]
    public class NewBaseType
    {
    }

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class Editor : NewBaseType, Form {
        public Editor() {
      InitializeComponent();

      var scrollUpdateTimer = new Timer();
      scrollUpdateTimer.Interval = 200;
      scrollUpdateTimer.Tick += (object sender, EventArgs e) => {
        UpdateScrollStates();
      };
      scrollUpdateTimer.Start();

      var compilerBinPath = Path.Combine(Directory.GetCurrentDirectory(),
                                         Path.GetDirectoryName(compilerPath_));
      compilerWatcher_ = new FileSystemWatcher(compilerBinPath, "*.exe");
      compilerWatcher_.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size;
      compilerWatcher_.Changed += (object sender, FileSystemEventArgs e) => {
        if (e.Name == Path.GetFileName(compilerPath_)) {
          Invoke((MethodInvoker)delegate {
            if (pendingTimer_) {
              return;
            }
            pendingTimer_ = true;
            var timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (object sender1, EventArgs e1) => {
              pendingTimer_ = false;
              timer.Dispose();
              Process(sourceCodeTextBox.Text);
            };
            timer.Start();
          });
        }
      };
      compilerWatcher_.EnableRaisingEvents = true;

      wordsTextBox.Click += (object sender, EventArgs e) => {
        wordsTextBox.SelectAll();
        wordsTextBox.Copy();
      };

      sourceCodeTextBox.Click += (object sender, EventArgs e) => {
        Process(sourceCodeTextBox.Text);
      };
      sourceCodeTextBox.TextChanged += (object sender, EventArgs e) => {
        Process(sourceCodeTextBox.Text);
      };

      translationComboBox.SelectedIndex = 0;
      translationComboBox.SelectedIndexChanged += (object sender, EventArgs e) => {
        Process(sourceCodeTextBox.Text);
      };
      vertexShaderComboBox.SelectedIndex = 0;
      vertexShaderComboBox.SelectedIndexChanged += (object sender, EventArgs e) => {
        Process(sourceCodeTextBox.Text);
      };

    sourceCodeTextBox.Text = string.Join(
        Environment.NewLine, new string[] {
"xps_3_0",
"dcl_texcoord1 r0",
"dcl_color r1.xy",
"exec",
"alloc colors",
"exec",
"tfetch1D r2, r0.y, tf0, FetchValidOnly=false",
"tfetch1D r2, r0.x, tf2",
"tfetch2D r3, r3.wx, tf13",
"tfetch2D r[aL+3], r[aL+5].wx, tf13, FetchValidOnly=false, UnnormalizedTextureCoords=true, MagFilter=linear, MinFilter=linear, MipFilter=point, AnisoFilter=max1to1, UseRegisterGradients=true, UseComputedLOD=false, UseRegisterLOD=true, OffsetX=-1.5, OffsetY=1.0",
"tfetch3D r31.w_01, r0.xyw, tf15",
"tfetchCube r5, r1.xyw, tf31",
"        setTexLOD r1.z",
"        setGradientH r1.zyx",
"(!p0)        setGradientV r1.zyx",
"        getGradients r5, r1.xy, tf3",
"        mad oC0, r0, r1.yyyy, c0",
"        mad oC0._, r0, r1.yyyy, c0",
"        mad oC0.x1_, r0, r1.yyyy, c0",
"        mad oC0.x10w, r0, r1.yyyy, c0",
"        mul r4.xyz, r1.xyzz, c5.xyzz",
"        mul r4.xyz, r1.xyzz, c[0 + aL].xyzz",
"        mul r4.xyz, r1.xyzz, c[6 + aL].xyzz",
"        mul r4.xyz, r1.xyzz, c[0 + a0].xyzz",
"        mul r4.xyz, r1.xyzz, c[8 + a0].xyzz",
"      + adds r5.w, r0.xz",
"        cos r6.w, r0.x",
"        adds r5.w, r0.zx",
"        mul r4.xyz, r[aL+1].xyzz, c[8 + a0].xyzz",
"        adds r5.w, r[aL+0].zx",
"        jmp l5",
"ccall b1, l5",
"nop",
"        label l5",
"(!p0)        exec",
"cexec b5, Yield=true",
"cexec !b6",
"        mulsc r3.w, c1.z, r1.w",
"loop i7, L4",
"   label L3",
"   exec",
"   setp_eq r15, c[aL].w",
"   (!p0) add r0, r0, c[aL]",
"(p0) endloop i7, L3",
"label L4",
"exece",
"        mulsc r3.w, c3.z, r6.x",
"        mulsc r3.w, c200.z, r31.x",
"        mov oDepth.x, c3.w",
"        cnop",
        });
    }

        private class NewBaseType
        {
            public override Stream Open(CompilerIncludeHandlerType includeType,
                                     string filename)
            {
                throw new NotImplementedException();
            }
        }

        [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
        class NopIncludeHandler : NewBaseType, CompilerIncludeHandler {
            private string GetDebuggerDisplay()
            {
                return ToString();
            }
        }

        public enum ScrollInfoMask : uint {
      SIF_RANGE = 0x1,
      SIF_PAGE = 0x2,
      SIF_POS = 0x4,
      SIF_DISABLENOSCROLL = 0x8,
      SIF_TRACKPOS = 0x10,
      SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS),
    }
    private struct ScrollInfo {
      public int cbSize;
      public uint fMask;
      public int nMin;
      public int nMax;
      public int nPage;
      public int nPos;
      public int nTrackPos;
    }
    }
}
