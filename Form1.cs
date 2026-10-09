using Lua;
using Lua.Standard;
using System.Diagnostics;
using System.Formats.Nrbf;
using System.Reflection.Emit;
using System.Xml.Linq;
using static Lua.CodeAnalysis.Syntax.DisplayStringSyntaxVisitor;
using Label = System.Windows.Forms.Label;

namespace Estrelas
{
    public partial class Form1 : Form
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        LuaState state = LuaState.Create();
        LuaValue[]? results = null;
        List<object> AllControls = new List<object>();
        List<LuaButton> Lbuttons = new List<LuaButton>();
        List<LuaLabel> Llabels = new List<LuaLabel>();
        List<LTxtInput> LtxtInputs = new List<LTxtInput>();

        string currentAddress = "";

        public  Form1(string address)
        {
            InitializeComponent();
            if (address != "")
            {
                textBox1.Text = address;
                currentAddress = address;
                if (Path.Exists(currentAddress))
                {
                    try
                    {
                        Clean();
                        InitSite(currentAddress, false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading site!");
                    }
                }
                else
                {
                    try
                    {
                        Clean();
                        InitSite(new HttpClient().GetStringAsync(currentAddress).Result, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading site!");
                    }
                }
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(dialog.FileName))
                {

                    InitSite(dialog.FileName, false);
                    currentAddress = dialog.FileName;
                }
                else
                {

                    MessageBox.Show("No site found!");
                }



            }
        }



        public async void InitSite(string path, bool isWeb)
        {
            try
            {
                textBox1.Text = currentAddress;


                // Clean();
                await Clean();
                state = LuaState.Create();
                state.OpenStandardLibraries();
           
                initLuaFuncs();
                if (isWeb)
                {
                    results = await state.DoStringAsync(path,cancellationToken: cts.Token);
                }
                else
                {
                    results = await state.DoFileAsync(path, cts.Token);
                }

                foreach (var cntrl in AllControls)
                {
                    if (cntrl is LuaButton)
                    {
                        //    var btn = (LuaButton)cntrl;
                        //    Debug.WriteLine($"About to create button {btn.name}");
                        //    createButton(btn);
                    }
                    else if (cntrl is LuaLabel)
                    {
                        //var lbl = (LuaLabel)cntrl;
                        //Debug.WriteLine($"About to create label {lbl.name}");
                        //createLabel(lbl);
                    }
                }

                timer1.Start();
            }
            catch (OperationCanceledException)
            {
                // shush
              
                Debug.WriteLine("Operation was cancelled");
                if (Path.Exists(currentAddress))
                {
                    try
                    {
              
                        InitSite(currentAddress, false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading site!");
                    }
                }
                else
                {
                    try
                    {
                   

                        InitSite(new HttpClient().GetStringAsync(currentAddress).Result, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading site!");
                    }
                }

            }
            catch (Exception ex)
            {
                if (!ex.Message.StartsWith("This state is running!"))
                {
                    MessageBox.Show("Sorry there seems to be something broken with this website! \n\n" + ex.Message);
                }
               
            }
        }

        async Task BLYAT(bool e, string adr)
        {
            await Task.Delay(100);
            if (e)
            {
            
                currentAddress = adr;
                InitSite(new HttpClient().GetStringAsync(adr).Result, true);

            }
            else
            {
              
                currentAddress = adr;
                InitSite(adr, false);

            }
        
        }

        private async void initLuaFuncs()
        {
            try
            {
                //
                // -- Misc --
                //

                state.Environment["wait"] = new LuaFunction(async (context, ct) =>
                {
                    var sec = context.GetArgument<double>(0);
                    Debug.WriteLine("initiating wait " + sec.ToString());

                    await Task.Delay(TimeSpan.FromSeconds(sec));
                    return context.Return();
                });
                state.Environment["output"] = new LuaFunction(async (context, ct) =>
                {
                    var write = context.GetArgument<string>(0);
                    Debug.WriteLine("initiating output " + write);
                    Console.WriteLine(write);
                    return context.Return();
                });
                state.Environment["msg"] = new LuaFunction(async (context, ct) =>
                {
                    var write = context.GetArgument<string>(0);
                    Debug.WriteLine("initiating msg " + write);
                    MessageBox.Show(write);
                    return context.Return();
                });

                //
                // -- Scroll --
                //

                state.Environment["scroll_up"] = new LuaFunction(async (context, ct) =>
                {

                    Debug.WriteLine("initiating scroll ");
                    flowLayoutPanel1.VerticalScroll.Value = flowLayoutPanel1.VerticalScroll.Minimum;
                    return context.Return();
                });
                state.Environment["scroll_down"] = new LuaFunction(async (context, ct) =>
                {

                    Debug.WriteLine("initiating scroll ");
                    flowLayoutPanel1.VerticalScroll.Value = flowLayoutPanel1.VerticalScroll.Maximum;
                    return context.Return();
                });
                state.Environment["auto_scroll"] = new LuaFunction(async (context, ct) =>
                {

                    Debug.WriteLine("initiating auto-scroll ");
                    var scr = context.GetArgument<bool>(0);
                    flowLayoutPanel1.AutoScroll = scr;
                    return context.Return();
                });

                //
                // -- Misc --
                //

                state.Environment["rand"] = new LuaFunction(async (context, ct) =>
                {
                    var min = context.GetArgument<int>(0);
                    var max = context.GetArgument<int>(1);
                    Debug.WriteLine("initiating random " + min + " to " + max);

                    return context.Return(new Random().Next(min, max));
                });

                //
                // -- IO --
                //

                state.Environment["read_file"] = new LuaFunction(async (context, ct) =>
                {
                    var path = context.GetArgument<string>(0);

                    Debug.WriteLine("initiating readfile " + path);

                    return context.Return(File.ReadAllText(path));
                });
                state.Environment["file_exists"] = new LuaFunction(async (context, ct) =>
                {
                    var path = context.GetArgument<string>(0);

                    Debug.WriteLine("initiating file_exists " + path);

                    return context.Return(File.Exists(path));
                });
                state.Environment["folder_exists"] = new LuaFunction(async (context, ct) =>
                {
                    var path = context.GetArgument<string>(0);

                    Debug.WriteLine("initiating folder_exists " + path);

                    return context.Return(Directory.Exists(path));
                });
                state.Environment["make_file"] = new LuaFunction(async (context, ct) =>
                {
                    var path = context.GetArgument<string>(0);

                    Debug.WriteLine("initiating make_file " + path);
                    File.WriteAllText(path, "");
                    return context.Return();
                });
                state.Environment["write_file"] = new LuaFunction(async (context, ct) =>
                {
                    var path = context.GetArgument<string>(0);
                    var content = context.GetArgument<string>(1);
                    Debug.WriteLine("initiating write_file " + path);
                    File.WriteAllText(path, content);
                    return context.Return();
                });
                state.Environment["make_folder"] = new LuaFunction(async (context, ct) =>
                {
                    var path = context.GetArgument<string>(0);

                    Debug.WriteLine("initiating make_folder " + path);
                    Directory.CreateDirectory(path);
                    return context.Return();
                });

                //
                // -- CMD --
                //

                state.Environment["cmd"] = new LuaFunction(async (context, ct) =>
                {
                    var cmd = context.GetArgument<string>(0);
                    Debug.WriteLine("initiating cmd " + cmd);
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c " + cmd;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = context.GetArgument<bool>(1);
                    Process process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                    if (context.GetArgument<bool>(2))
                    {
                        process.WaitForExit();
                    }
                    return context.Return();
                });
                state.Environment["ps"] = new LuaFunction(async (context, ct) =>
                {
                    var cmd = context.GetArgument<string>(0);
                    Debug.WriteLine("initiating ps " + cmd);
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.Arguments = "/c " + cmd;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = context.GetArgument<bool>(1);
                    Process process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                    if (context.GetArgument<bool>(2))
                    {
                        process.WaitForExit();
                    }
                    return context.Return();
                });
                state.Environment["run"] = new LuaFunction(async (context, ct) =>
                {
                    var cmd = context.GetArgument<string>(0);
                    Debug.WriteLine("initiating cmd " + cmd);
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = cmd;
                    psi.Arguments = "";

                    psi.RedirectStandardOutput = false;
                    psi.RedirectStandardError = false;
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = false;
                    Process process = new Process();
                    process.StartInfo = psi;
                    process.Start();
                    return context.Return();
                });

                //
                // -- WEB --
                //

                try
                {
                    state.Environment["redirect"] = new LuaFunction(async (context, ct) =>
                    {
                        var adr = context.GetArgument<string>(0);
                        BLYAT(context.GetArgument<bool>(1), adr);
                        //if (context.GetArgument<bool>(1))
                        //{
                        //    Clean();
                        //    currentAddress = adr;
                        //    InitSite(new HttpClient().GetStringAsync(adr).Result, true);

                        //}
                        //else
                        //{
                        //    Clean();
                        //    currentAddress = adr;
                        //    InitSite(adr, false);

                        //}


                        return context.Return("blin");
                    });
                }
                catch (NullReferenceException ex)
                {
                 
                }
                state.Environment["current_address"] = currentAddress;

                //
                // -- HTTP --
                //

                state.Environment["http_get"] = new LuaFunction(async (context, ct) =>
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Estrelas/1.0");
           



                    return context.Return(client.GetStringAsync(context.GetArgument<string>(0)).Result);
                });
                state.Environment["http_post"] = new LuaFunction(async (context, ct) =>
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "Estrelas/1.0");


                    var content = new StringContent(context.GetArgument<string>(1), System.Text.Encoding.UTF8, "application/json");


                    return context.Return(client.PostAsync(context.GetArgument<string>(0), content).Result.ToString());
                });
                //
                // -- UI controls --
                //

                state.Environment["remove_all"] = new LuaFunction(async (context, ct) =>
                {

                    Lbuttons.Clear();
                    Llabels.Clear();
                    LtxtInputs.Clear();
                    AllControls.Clear();
                    flowLayoutPanel1.Controls.Clear();


                    return context.Return();
                });

                state.Environment["remove_btn"] = new LuaFunction(async (context, ct) =>
                {
                    var btn = context.GetArgument<LuaButton>(0);
                    Lbuttons.Remove(btn);
                    AllControls.Remove(btn);
                    flowLayoutPanel1.Controls.Remove(btn.btn);


                    return context.Return();
                });

                state.Environment["remove_label"] = new LuaFunction(async (context, ct) =>
                {
                    var lbl = context.GetArgument<LuaLabel>(0);
                    Llabels.Remove(lbl);
                    AllControls.Remove(lbl);
                    flowLayoutPanel1.Controls.Remove(lbl.lbl);


                    return context.Return();
                });

                state.Environment["remove_txtbox"] = new LuaFunction(async (context, ct) =>
                {
                    var txtbox = context.GetArgument<LTxtInput>(0);
                    LtxtInputs.Remove(txtbox);
                    AllControls.Remove(txtbox);
                    flowLayoutPanel1.Controls.Remove(txtbox.txt);


                    return context.Return();
                });
                state.Environment["create_button"] = new LuaFunction(async (context, ct) =>
                {
                    var s2 = context.GetArgument<string>(0);
                    string? id = null;
                    try
                    {
                        id = context.GetArgument<string>(1);
                    }
                    catch (Exception ex)
                    {
                        id = null;
                    }
                    var width = context.GetArgument<int>(2);
                    var height = context.GetArgument<int>(3);
                    LuaButton lb = new LuaButton(s2, id, width, height);
                    state.Environment[$"button{id}"] = lb;
                    AllControls.Add(lb);
                    Lbuttons.Add(lb);
                    createButton(lb);
                    return context.Return();
                });
                state.Environment["create_label"] = new LuaFunction(async (context, ct) =>
                {
                    var s2 = context.GetArgument<string>(0);
                    string? id = null;
                    try
                    {
                        id = context.GetArgument<string>(1);
                    }
                    catch (Exception ex)
                    {
                        id = null;

                    }

                    int fnt = context.GetArgument<int>(2);
                    LuaLabel lbl = new LuaLabel(s2, id, fnt);
                    state.Environment[$"label{id}"] = lbl;
                    AllControls.Add(lbl);
                    Llabels.Add(lbl);
                    createLabel(lbl);
                    return context.Return();
                });
                state.Environment["create_textbox"] = new LuaFunction(async (context, ct) =>
                {
                    var s2 = context.GetArgument<string>(0);
                    string? id = null;
                    try
                    {
                        id = context.GetArgument<string>(1);
                    }
                    catch (Exception ex)
                    {
                        id = null;

                    }

                    int fnt = context.GetArgument<int>(2);
                    LTxtInput lbl = new LTxtInput(s2, id, fnt);
                    state.Environment[$"textbox{id}"] = lbl;
                    AllControls.Add(lbl);
                    LtxtInputs.Add(lbl);
                    createTxtInput(lbl);
                    return context.Return();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lua error within website!");
            }

        }
        private async void createButton(LuaButton buton)
        {
            try
            {
                Button button = new Button();
                button.Text = buton.name;
                Size a = TextRenderer.MeasureText(buton.name, button.Font);
                button.Width = a.Width + 10;
                button.Height = a.Height + 10;

                if (buton.width != 0)
                {
                    button.Width = buton.width;
                }
                if (buton.height != 0)
                {
                    button.Height = buton.height;
                }
                buton.height = button.Height;
                buton.width = button.Width;

                button.Click += async (e, s) =>
                {
                    var funct = state.Environment[$"btn{buton.id}"];
                    try
                    {
                        if (funct.Type == LuaValueType.Function)
                        {
                            var ret = await state.CallAsync(funct, []);

                        }
                    }
                    catch (Exception ex)
                    {
                       
                    }
                };
                button.MouseHover += async (e, s) =>
                {
                    var funct = state.Environment[$"btn{buton.id}_hover"];

                    if (funct.Type == LuaValueType.Function)
                    {
                        var ret = await state.CallAsync(funct, []);

                    }
                };

                buton.btn = button;

                flowLayoutPanel1.Controls.Add(button);
                Debug.WriteLine($"Created button {buton.name} w: {buton.width} h: {button.Height}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error interatcting/creating button");
            }
        }
        private async void createLabel(LuaLabel h1)
        {
            try
            {
                Label label = new Label();
                label.Text = h1.name;
                if (h1.fontScale != 0)
                {
                    Font fnt = new Font(label.Font.FontFamily, h1.fontScale, label.Font.Style);
                    label.Font = fnt;
                }
                Size a = TextRenderer.MeasureText(h1.name, label.Font);
                label.Width = a.Width + 10;
                label.Height = a.Height + 10;
                h1.fontScale = (int)label.Font.Size;
                label.Click += async (e, s) =>
                {
                    var funct = state.Environment[$"lbl{h1.id}"];

                    if (funct.Type == LuaValueType.Function)
                    {
                        var ret = await state.CallAsync(funct, []);

                    }
                };
                label.MouseHover += async (e, s) =>
                {
                    var funct = state.Environment[$"lbl{h1.id}_hover"];

                    if (funct.Type == LuaValueType.Function)
                    {
                        var ret = await state.CallAsync(funct, []);

                    }
                };
                h1.lbl = label;

                flowLayoutPanel1.Controls.Add(label);
                Debug.WriteLine($"Created Label {h1.name} pt: {label.Font.Size}");
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error interacting/creating label");
            }
        }
        private async void createTxtInput(LTxtInput h1)
        {
            try
            {
                TextBox textBox = new TextBox();
                textBox.Text = h1.name;
                if (h1.fontScale != 0)
                {
                    Font fnt = new Font(textBox.Font.FontFamily, h1.fontScale, textBox.Font.Style);
                    textBox.Font = fnt;
                }
                Size a = TextRenderer.MeasureText(h1.name, textBox.Font);
                textBox.Width = a.Width + 10;
                textBox.Height = a.Height + 10;
                h1.fontScale = (int)textBox.Font.Size;
                textBox.TextChanged += async (e, s) =>
                {
                    h1.name = textBox.Text;
                    var funct = state.Environment[$"txtbox{h1.id}_change"];
                    if (funct.Type == LuaValueType.Function)
                    {
                        var ret = await state.CallAsync(funct, [textBox.Text]);
                    }
                };
                textBox.Click += async (e, s) =>
                {
                    var funct = state.Environment[$"txtbox{h1.id}"];

                    if (funct.Type == LuaValueType.Function)
                    {
                        var ret = await state.CallAsync(funct, []);

                    }
                };
                textBox.MouseHover += async (e, s) =>
                {
                    var funct = state.Environment[$"txtbox{h1.id}_hover"];

                    if (funct.Type == LuaValueType.Function)
                    {
                        var ret = await state.CallAsync(funct, []);

                    }
                };
                h1.txt = textBox;

                flowLayoutPanel1.Controls.Add(textBox);
                Debug.WriteLine($"Created TextBox {h1.name} pt: {textBox.Font.Size}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error interacting/creating text box");
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("ticking");
            Update();
        }
        public void Update()
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Button)
                {

                    var something2 = Lbuttons.Where(x => x.btn == control).FirstOrDefault();
                    if (control.Text != something2.name)
                        control.Text = something2.name;

                    if (control.Width != something2.width)
                        control.Width = something2.width;

                    if (control.Height != something2.height)
                        control.Height = something2.height;
                }
                else if (control is Label)
                {

                    var something2 = Llabels.Where(x => x.lbl == control).FirstOrDefault();

                    if (control.Font.Size != something2.fontScale || control.Text != something2.name)
                    {

                        control.Text = something2.name;
                        Font fnt = new Font(control.Font.FontFamily, something2.fontScale, control.Font.Style);
                        control.Font = fnt;
                        Size a = TextRenderer.MeasureText(something2.name, control.Font);
                        control.Width = a.Width + 10;
                        control.Height = a.Height + 10;
                    }

                }
                else if (control is TextBox)
                {

                    var something2 = LtxtInputs.Where(x => x.txt == control).FirstOrDefault();

                    if (control.Font.Size != something2.fontScale || control.Text != something2.name)
                    {

                        control.Text = something2.name;
                        Font fnt = new Font(control.Font.FontFamily, something2.fontScale, control.Font.Style);
                        control.Font = fnt;
                        Size a = TextRenderer.MeasureText(something2.name, control.Font);
                        control.Width = a.Width + 10;
                        control.Height = a.Height + 10;
                    }

                }
            }
        }


        public async Task Clean()
        {

            timer1.Stop();
            AllControls.Clear();
            Llabels.Clear();
            Lbuttons.Clear();
            LtxtInputs.Clear();
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                control.Dispose();
            }
            foreach (var cntrl in AllControls)
            {
                if (cntrl is LuaButton)
                {
                    var btn = (LuaButton)cntrl;
                    btn.btn.Dispose();
                }
                else if (cntrl is LuaLabel)
                {
                    var lbl = (LuaLabel)cntrl;
                    lbl.lbl.Dispose();
                }
                else if (cntrl is LTxtInput)
                {
                    var txt = (LTxtInput)cntrl;
                    txt.txt.Dispose();
                }
            }
            if (state != null)
            {
                cts.Cancel();
                state.Dispose();
                state = null;
                cts.Dispose();
                cts = new CancellationTokenSource();
            }
            results = null;
           

           


            flowLayoutPanel1.Controls.Clear();
        }


        private void button3_Click(object sender, EventArgs e)
        {


            if (Path.Exists(currentAddress))
            {
                try
                {
                    Clean();
                     
                    InitSite(currentAddress, false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading site!");
                }
            }
            else
            {
                try
                {
                    Clean();
                     
                    InitSite(new HttpClient().GetStringAsync(currentAddress).Result, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading site!");
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                currentAddress = textBox1.Text;

                if (Path.Exists(currentAddress))
                {
                    try
                    {
                        Clean();
                         
                        InitSite(currentAddress, false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading site!");
                    }
                }
                else
                {
                    try
                    {
                        Clean();
                         
                        InitSite(new HttpClient().GetStringAsync(currentAddress).Result, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading site!");
                    }
                }


            }
            else
            {
                MessageBox.Show("Please enter a valid path!");
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void button4_Click(object sender, EventArgs e)
        {
      
           await Clean();
         
            currentAddress = "https://bwe.aquaweb.cc/another.lua";
            InitSite(new HttpClient().GetStringAsync(currentAddress).Result, true);
        }
    }
}
