using Lua;
using Lua.Standard;
using System.Diagnostics;
using System.Formats.Nrbf;
using System.Reflection.Emit;
using System.Xml.Linq;
using Label = System.Windows.Forms.Label;

namespace Estrelas
{
    public partial class Form1 : Form
    {
        string basePath = "";
        string extPath = "index";
        LuaState state = LuaState.Create();
        LuaValue[]? results = null;
        List<object> AllControls = new List<object>();
        List<LuaButton> Lbuttons = new List<LuaButton>();
        List<LuaLabel> Llabels = new List<LuaLabel>();
        public Form1()
        {
            InitializeComponent();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (Path.Exists(dialog.SelectedPath) && File.Exists(Path.Combine(dialog.SelectedPath, "index.lua")))
                {
                    basePath = dialog.SelectedPath;
                    extPath = "index";
                    InitSite(address(extPath));
                }
                else
                {

                    MessageBox.Show("No site found!");
                }



            }
        }

        string address(string name)
        {
            return Path.Combine(basePath, name);
        }

        public async void InitSite(string path)
        {
            try
            {
             
                string luas = path + ".lua";
            
                Clean();
                state = LuaState.Create();
                state.OpenStandardLibraries();
               
                initLuaFuncs();
                results = await state.DoFileAsync(luas);
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
            catch (Exception ex)
            {
                MessageBox.Show("Sorry there seems to be something broken with this website!");
            }
        }

        private async void initLuaFuncs()
        {
            try
            {
                state.Environment["wait"] = new LuaFunction(async (context, ct) =>
                {
                    var sec = context.GetArgument<double>(0);
                    await Task.Delay(TimeSpan.FromSeconds(sec));
                    return context.Return();
                });
                state.Environment["output"] = new LuaFunction(async (context, ct) =>
                {
                    var write = context.GetArgument<string>(0);
                    Console.WriteLine(write);
                    return context.Return();
                });
                state.Environment["msg"] = new LuaFunction(async (context, ct) =>
                {
                    var write = context.GetArgument<string>(0);
                    MessageBox.Show(write);
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
            }
            catch (Exception ex) {
                MessageBox.Show("Lua error within website!");
            }

        }
        private async void createButton(LuaButton buton)
        {
            try {
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

                if (funct.Type == LuaValueType.Function)
                {
                    var ret = await state.CallAsync(funct, []);

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
                MessageBox.Show("Error interatcting/creating label");
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
                else if (control is Label) {

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
                }
            }
        

        public void Clean()
        {
            timer1.Stop();
            AllControls.Clear();
            Llabels.Clear();
            state = null;
            Lbuttons.Clear();
            results = null;
            flowLayoutPanel1.Controls.Clear();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            Clean();
               InitSite(address(extPath));
        }
    }
}
