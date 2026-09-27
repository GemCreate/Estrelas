using Lua;
using Lua.Standard;
using System.Diagnostics;
using System.Formats.Nrbf;
using System.Xml.Linq;

namespace Estrelas
{
    public partial class Form1 : Form
    {
        string basePath = "";
        LuaState state = LuaState.Create();
        LuaValue[]? results = null;

        List<LuaButton> Lbuttons = new List<LuaButton>();
        public Form1()
        {
            InitializeComponent();
         
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (Path.Exists(dialog.SelectedPath) && File.Exists(Path.Combine(dialog.SelectedPath, "index.ewl")))
                {
                    basePath = dialog.SelectedPath;
                    InitSite(address("index"));
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
            string comps = File.ReadAllText(path + ".ewl");
            string luas = path + ".lua";
            string[] cleanComps = comps.Trim().Split('\n');
            state = null;
            state = LuaState.Create();
            state.OpenStandardLibraries();
            foreach (string cleanComp in cleanComps)
            {

                 Debug.WriteLine($"Reading line: {cleanComp}");
                if (cleanComp.StartsWith("button "))
                {
                     Debug.WriteLine("Found a button component");
                    string s1 = cleanComp.Substring(7);
                    string s2 = cleanComp.Split("-text: ").Last().Replace("-text: ", "");
                    string[] args = cleanComp.Split("-text: ").First().Replace("-text: ", "").Split(" ");
                    int width = 0;
                    int height = 0;
                    string id = "";
                    foreach (string arg in args)
                    {
                        if (arg.StartsWith("-id:"))
                        {
                            id = arg.Substring(4);
                        }
                        else if (arg.StartsWith("-wd:"))
                        {
                            width = int.Parse(arg.Substring(4));
                        }
                        else if (arg.StartsWith("-hg:"))
                        {
                            height = int.Parse(arg.Substring(4));
                        }

                    }
                    LuaButton lb = new LuaButton(s2, id, width, height);
                    state.Environment[$"button{id}"] = lb;
                    Lbuttons.Add(lb);

                     Debug.WriteLine($"Found button {s2} w: {width} h: {Height}");
          
                }


            }
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
            results = await state.DoFileAsync(luas);
            foreach (LuaButton button in Lbuttons)
            {
                 Debug.WriteLine($"About to create button {button.name}");
                createButton(button);
            }

            timer1.Start();
        }


        private async void createButton(LuaButton buton)
        {
            Button button = new Button();
            button.Text = buton.name;
            button.Size = TextRenderer.MeasureText(buton.name, button.Font);
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
            buton.btn = button;

            flowLayoutPanel1.Controls.Add(button);
             Debug.WriteLine($"Created button {buton.name} w: {buton.width} h: {button.Height}");

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
             Debug.WriteLine("ticking");
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
            }
        }
    }
}
