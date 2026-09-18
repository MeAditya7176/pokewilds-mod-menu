using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[assembly: AssemblyTitle("PokeWilds Mod Menu and Spawner")]
[assembly: AssemblyDescription("Mod Menu, Pokemon Spawner and Save Editor for PokeWilds")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("PokeWilds Community")]
[assembly: AssemblyProduct("PokeWilds Mod Menu")]
[assembly: AssemblyCopyright("Copyright (C) 2026 PokeWilds Community")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Runtime.InteropServices.Guid("e2b5c7a1-8f34-4d92-b106-7e5c9a124d3f")]
[assembly: AssemblyVersion("1.5.0.0")]
[assembly: AssemblyFileVersion("1.5.0.0")]

namespace PokeWildsModMenu
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ModMenuForm());
            }
            catch (Exception ex)
            {
                File.WriteAllText("modmenu_error.log", ex.ToString());
                MessageBox.Show(ex.Message, "PokeWilds Mod Menu Error");
            }
        }
    }

    public class ModMenuForm : Form
    {
        private string gameDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
        private string activeSavFolder = "";
        private string activeZipPath = "";
        private Dictionary<string, object> saveData = null;
        private JavaScriptSerializer jsonSer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

        // Top UI Controls
        private ComboBox cmbSaves;
        private Label lblPlayerInfo;
        private Label lblStatus;
        private TabControl tabControl;
        private Button btnNavItems;
        private Button btnNavPokemon;
        private Button btnNavWorld;

        // Items Tab
        private ListView listItems;
        private ComboBox cmbItemSelect;
        private NumericUpDown numItemQty;

        // Pokemon Tab
        private ListBox listParty;
        private TextBox txtPokeNick;
        private NumericUpDown numPokeLevel;
        private NumericUpDown numPokeHp;
        private CheckBox chkPokeShiny;
        private NumericUpDown numPokeFriend;

        // Pokemon Spawner Controls
        private ComboBox cmbSpawnPoke;
        private NumericUpDown numSpawnLevel;
        private CheckBox chkSpawnShiny;
        private ComboBox cmbSpawnGender;
        private TextBox txtSpawnNick;

        // World Tab
        private RadioButton radDay;
        private RadioButton radNight;
        private RadioButton radDusk;

        private readonly string[] allItems = new string[] {
            "master ball", "ultra ball", "great ball", "poke ball", "dusk ball", "heavy ball", 
            "moon ball", "love ball", "fast ball", "level ball", "lure ball", "friend ball", 
            "net ball", "dive ball", "quick ball", "heal ball", "nest ball", "timer ball", "dream ball",
            "rare candy", "ragecandybar", "berry juice", "escape rope", "sleeping bag",
            "fire stone", "water stone", "thunderstone", "leaf stone", "moon stone", "sun stone", 
            "dawn stone", "dusk stone", "shiny stone", "ice stone", "hard stone", "oval stone", "everstone",
            "lum berry", "pecha berry", "cheri berry", "aspear berry", "chesto berry", "rawst berry", "persim berry",
            "wood", "soft soil", "brick", "charcoal", "fertilizer", "silk", "metal coat", "dragon scale", "kings rock", "life force"
        };

        private List<string> allPokemonNames = new List<string>();
        private Dictionary<string, List<KeyValuePair<int, string>>> learnsets = new Dictionary<string, List<KeyValuePair<int, string>>>(StringComparer.OrdinalIgnoreCase);

        public ModMenuForm()
        {
            this.Text = "PokéWilds Mod Menu & Spawner v1.5";
            this.Size = new Size(910, 680);
            this.MinimumSize = new Size(910, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 26, 32);
            this.ForeColor = Color.FromArgb(240, 240, 245);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Robust game directory auto-detection
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDir = !string.IsNullOrEmpty(exePath) ? Path.GetDirectoryName(exePath) : "";
                string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');

                if (!string.IsNullOrEmpty(exeDir) && (Directory.Exists(Path.Combine(exeDir, "app")) || File.Exists(Path.Combine(exeDir, "pokewilds.exe")) || Directory.GetDirectories(exeDir, "*.sav").Length > 0))
                {
                    gameDir = exeDir;
                }
                else if (Directory.Exists(Path.Combine(appDir, "app")) || File.Exists(Path.Combine(appDir, "pokewilds.exe")) || Directory.GetDirectories(appDir, "*.sav").Length > 0)
                {
                    gameDir = appDir;
                }
            }
            catch { }

            LoadPokemonNames();
            LoadLearnsets();

            // Set Form Icon
            try
            {
                string icoFile = Path.Combine(gameDir, "app_logo.ico");
                if (!File.Exists(icoFile)) icoFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_logo.ico");
                if (File.Exists(icoFile))
                {
                    this.Icon = new Icon(icoFile);
                }
                else
                {
                    this.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
                }
            }
            catch { }

            InitUI();
            ScanSaves();
        }

        private void LoadPokemonNames()
        {
            string pFile = Path.Combine(gameDir, "pokemon_list.txt");
            if (!File.Exists(pFile)) pFile = @"C:\Users\Admin\.gemini\antigravity-ide\scratch\all_pokemon.txt";
            if (File.Exists(pFile))
            {
                string[] lines = File.ReadAllLines(pFile);
                foreach (string l in lines)
                {
                    string t = l.Trim().ToLower();
                    if (!string.IsNullOrEmpty(t) && !allPokemonNames.Contains(t))
                    {
                        allPokemonNames.Add(t);
                    }
                }
            }

            if (allPokemonNames.Count == 0)
            {
                allPokemonNames.AddRange(new string[] {
                    "bulbasaur", "ivysaur", "venusaur", "charmander", "charmeleon", "charizard",
                    "squirtle", "wartortle", "blastoise", "pikachu", "raichu", "mewtwo", "mew",
                    "machop", "machoke", "machamp", "gengar", "rayquaza", "lucario", "eevee"
                });
            }
        }

        private void LoadLearnsets()
        {
            string lFile = Path.Combine(gameDir, "pokemon_learnsets.txt");
            if (!File.Exists(lFile)) lFile = @"C:\Users\Admin\.gemini\antigravity-ide\scratch\learnsets.txt";
            if (File.Exists(lFile))
            {
                try
                {
                    string[] lines = File.ReadAllLines(lFile);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrEmpty(line) || !line.Contains("=")) continue;
                        string[] parts = line.Split('=');
                        string mon = parts[0].Trim().ToLower();
                        string[] mvList = parts[1].Split(';');

                        List<KeyValuePair<int, string>> moves = new List<KeyValuePair<int, string>>();
                        foreach (string mv in mvList)
                        {
                            if (!mv.Contains(":")) continue;
                            string[] p = mv.Split(':');
                            int lvl;
                            if (int.TryParse(p[0], out lvl))
                            {
                                moves.Add(new KeyValuePair<int, string>(lvl, p[1].Trim().ToLower()));
                            }
                        }
                        learnsets[mon] = moves;
                    }
                }
                catch { }
            }
        }

        private List<string> GetDefaultMovesForSpecies(string species, int level)
        {
            species = species.Trim().ToLower();
            List<string> result = new List<string>();

            if (learnsets.ContainsKey(species))
            {
                var moves = learnsets[species];
                List<string> learned = new List<string>();
                foreach (var pair in moves)
                {
                    if (pair.Key <= level)
                    {
                        // Add move if not already present or append
                        learned.Add(pair.Value);
                    }
                }

                if (learned.Count > 0)
                {
                    // Unique moves in reverse order (most recent)
                    List<string> uniqueMoves = new List<string>();
                    for (int i = learned.Count - 1; i >= 0 && uniqueMoves.Count < 4; i--)
                    {
                        if (!uniqueMoves.Contains(learned[i]))
                        {
                            uniqueMoves.Insert(0, learned[i]);
                        }
                    }
                    return uniqueMoves;
                }
            }

            // General fallback moves
            if (species.Contains("char") || species.Contains("fire"))
                return new List<string> { "scratch", "growl", "ember", "flamethrower" };
            if (species.Contains("water") || species.Contains("squirt"))
                return new List<string> { "tackle", "tail whip", "bubble", "water gun" };
            if (species.Contains("grass") || species.Contains("bulb"))
                return new List<string> { "tackle", "growl", "leech seed", "vine whip" };
            if (species.Contains("machop") || species.Contains("fight"))
                return new List<string> { "low kick", "leer", "karate chop", "seismic toss" };
            if (species.Contains("mew"))
                return new List<string> { "confusion", "psychic", "swift", "barrier" };

            return new List<string> { "tackle", "growl" };
        }

        private void InitUI()
        {
            // Top Panel (Header)
            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 118;
            topPanel.BackColor = Color.FromArgb(32, 35, 44);
            this.Controls.Add(topPanel);

            // Top Left Logo
            PictureBox picLogo = new PictureBox();
            picLogo.Location = new Point(16, 10);
            picLogo.Size = new Size(46, 46);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            string logoPng = Path.Combine(gameDir, "app_logo.png");
            if (!File.Exists(logoPng)) logoPng = @"C:\Users\Admin\.gemini\antigravity-ide\scratch\app_logo.png";
            if (File.Exists(logoPng))
            {
                try { picLogo.Image = Image.FromFile(logoPng); } catch { }
            }
            topPanel.Controls.Add(picLogo);

            Label lblTitle = new Label();
            lblTitle.Text = "⚡ PokéWilds Mod Menu & Spawner";
            lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(129, 140, 248);
            lblTitle.Location = new Point(70, 10);
            lblTitle.AutoSize = true;
            topPanel.Controls.Add(lblTitle);

            Label lblSelectWorld = new Label();
            lblSelectWorld.Text = "Active World:";
            lblSelectWorld.Location = new Point(510, 16);
            lblSelectWorld.AutoSize = true;
            topPanel.Controls.Add(lblSelectWorld);

            cmbSaves = new ComboBox();
            cmbSaves.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSaves.Location = new Point(595, 13);
            cmbSaves.Width = 165;
            cmbSaves.BackColor = Color.FromArgb(45, 49, 60);
            cmbSaves.ForeColor = Color.White;
            cmbSaves.SelectedIndexChanged += delegate { LoadSelectedSave(); };
            topPanel.Controls.Add(cmbSaves);

            Button btnRefresh = CreateStyledButton("🔄 Refresh", 770, 11, 95, 28, Color.FromArgb(59, 130, 246));
            btnRefresh.Click += delegate { ScanSaves(); };
            topPanel.Controls.Add(btnRefresh);

            lblPlayerInfo = new Label();
            lblPlayerInfo.Text = "Player: Loading... | No Save Loaded";
            lblPlayerInfo.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblPlayerInfo.ForeColor = Color.FromArgb(156, 163, 175);
            lblPlayerInfo.Location = new Point(72, 38);
            lblPlayerInfo.AutoSize = true;
            topPanel.Controls.Add(lblPlayerInfo);

            // Row 3: 3 Navigation Tab Buttons (Equally distributed)
            btnNavItems = CreateStyledButton("🎒 Items & Bag", 18, 70, 270, 38, Color.FromArgb(99, 102, 241));
            btnNavItems.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavItems.Click += delegate { SwitchTab(0); };
            topPanel.Controls.Add(btnNavItems);

            btnNavPokemon = CreateStyledButton("⭐ Pokémon Spawner", 305, 70, 275, 38, Color.FromArgb(45, 49, 60));
            btnNavPokemon.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavPokemon.Click += delegate { SwitchTab(1); };
            topPanel.Controls.Add(btnNavPokemon);

            btnNavWorld = CreateStyledButton("🌍 World & Time", 595, 70, 270, 38, Color.FromArgb(45, 49, 60));
            btnNavWorld.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavWorld.Click += delegate { SwitchTab(2); };
            topPanel.Controls.Add(btnNavWorld);

            // Bottom Status & Action Panel
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 65;
            bottomPanel.BackColor = Color.FromArgb(32, 35, 44);
            this.Controls.Add(bottomPanel);

            lblStatus = new Label();
            lblStatus.Text = "Ready to modify PokéWilds saves!";
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblStatus.ForeColor = Color.FromArgb(156, 163, 175);
            lblStatus.Location = new Point(16, 10);
            lblStatus.AutoSize = true;
            bottomPanel.Controls.Add(lblStatus);

            Label lblReloadTip = new Label();
            lblReloadTip.Text = "💡 Tip: After saving in Mod Menu, press Save in-game -> Title -> Continue to load changes!";
            lblReloadTip.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            lblReloadTip.ForeColor = Color.FromArgb(251, 191, 36);
            lblReloadTip.Location = new Point(16, 32);
            lblReloadTip.AutoSize = true;
            bottomPanel.Controls.Add(lblReloadTip);

            Button btnSaveAll = CreateStyledButton("💾 Apply & Save to Game", 620, 12, 255, 40, Color.FromArgb(16, 185, 129));
            btnSaveAll.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnSaveAll.Click += delegate { ApplyAndSave("All Changes Saved Successfully!"); };
            bottomPanel.Controls.Add(btnSaveAll);

            // Tab Control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
            this.Controls.Add(tabControl);

            InitItemsTab();
            InitPokemonTab();
            InitWorldTab();

            // Bring tab control forward
            tabControl.BringToFront();
        }

        private void SwitchTab(int index)
        {
            tabControl.SelectedIndex = index;
            btnNavItems.BackColor = index == 0 ? Color.FromArgb(99, 102, 241) : Color.FromArgb(45, 49, 60);
            btnNavPokemon.BackColor = index == 1 ? Color.FromArgb(99, 102, 241) : Color.FromArgb(45, 49, 60);
            btnNavWorld.BackColor = index == 2 ? Color.FromArgb(99, 102, 241) : Color.FromArgb(45, 49, 60);

            // Always sync fresh save data from disk when navigating tabs!
            LoadSelectedSave();
        }

        private Button CreateStyledButton(string text, int x, int y, int w, int h, Color bg)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            return btn;
        }

        // ----------------------------------------------------
        // TAB 1: ITEMS & INVENTORY
        // ----------------------------------------------------
        private void InitItemsTab()
        {
            TabPage tab = new TabPage("Items");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            // Left Column: Quick 1-Click Cheats
            GroupBox grpQuick = new GroupBox();
            grpQuick.Text = "⚡ Quick 1-Click Item Cheats";
            grpQuick.ForeColor = Color.FromArgb(129, 140, 248);
            grpQuick.Location = new Point(15, 10);
            grpQuick.Size = new Size(245, 440);
            tab.Controls.Add(grpQuick);

            Button btn99Master = CreateStyledButton("★ +99 Master Balls", 15, 30, 215, 36, Color.FromArgb(147, 51, 234));
            btn99Master.Click += delegate { AddItemDirectly("master ball", 99); };
            grpQuick.Controls.Add(btn99Master);

            Button btn99Candy = CreateStyledButton("🍬 +99 Rare Candies", 15, 75, 215, 36, Color.FromArgb(236, 72, 153));
            btn99Candy.Click += delegate { AddItemDirectly("rare candy", 99); };
            grpQuick.Controls.Add(btn99Candy);

            Button btnStones = CreateStyledButton("💎 +20 All Evo Stones", 15, 120, 215, 36, Color.FromArgb(6, 182, 212));
            btnStones.Click += delegate { AddAllEvoStones(20); };
            grpQuick.Controls.Add(btnStones);

            Button btnBerries = CreateStyledButton("🌿 +50 Berries & Ropes", 15, 165, 215, 36, Color.FromArgb(16, 185, 129));
            btnBerries.Click += delegate { AddBerriesAndRopes(50); };
            grpQuick.Controls.Add(btnBerries);

            Button btnSpecialBalls = CreateStyledButton("🎯 +50 All Special Poké Balls", 15, 210, 215, 36, Color.FromArgb(245, 158, 11));
            btnSpecialBalls.Click += delegate { AddAllPokeballs(50); };
            grpQuick.Controls.Add(btnSpecialBalls);

            Button btnMaxAll = CreateStyledButton("📦 Max All Items to 999", 15, 255, 215, 36, Color.FromArgb(239, 68, 68));
            btnMaxAll.Click += delegate { MaxOutAllItems(); };
            grpQuick.Controls.Add(btnMaxAll);

            // Right Column: Custom Item Adder & Bag Contents
            GroupBox grpCustom = new GroupBox();
            grpCustom.Text = "➕ Custom Item Adder";
            grpCustom.ForeColor = Color.FromArgb(129, 140, 248);
            grpCustom.Location = new Point(275, 10);
            grpCustom.Size = new Size(595, 85);
            tab.Controls.Add(grpCustom);

            Label lblItem = new Label();
            lblItem.Text = "Select Item:";
            lblItem.Location = new Point(15, 22);
            lblItem.AutoSize = true;
            grpCustom.Controls.Add(lblItem);

            cmbItemSelect = new ComboBox();
            cmbItemSelect.Location = new Point(15, 45);
            cmbItemSelect.Width = 220;
            cmbItemSelect.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbItemSelect.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbItemSelect.BackColor = Color.FromArgb(45, 49, 60);
            cmbItemSelect.ForeColor = Color.White;
            foreach (string itm in allItems) cmbItemSelect.Items.Add(itm);
            if (cmbItemSelect.Items.Count > 0) cmbItemSelect.SelectedIndex = 0;
            grpCustom.Controls.Add(cmbItemSelect);

            Label lblQty = new Label();
            lblQty.Text = "Quantity:";
            lblQty.Location = new Point(250, 22);
            lblQty.AutoSize = true;
            grpCustom.Controls.Add(lblQty);

            numItemQty = new NumericUpDown();
            numItemQty.Location = new Point(250, 45);
            numItemQty.Width = 75;
            numItemQty.Minimum = 1;
            numItemQty.Maximum = 999;
            numItemQty.Value = 99;
            numItemQty.BackColor = Color.FromArgb(45, 49, 60);
            numItemQty.ForeColor = Color.White;
            grpCustom.Controls.Add(numItemQty);

            Button btnAddCustom = CreateStyledButton("➕ Add to Bag", 345, 43, 175, 28, Color.FromArgb(16, 185, 129));
            btnAddCustom.Click += delegate {
                if (cmbItemSelect.SelectedItem != null)
                {
                    AddItemDirectly(cmbItemSelect.SelectedItem.ToString(), (int)numItemQty.Value);
                }
            };
            grpCustom.Controls.Add(btnAddCustom);

            // Bag Viewer
            GroupBox grpBag = new GroupBox();
            grpBag.Text = "🎒 Current Bag Contents";
            grpBag.ForeColor = Color.FromArgb(129, 140, 248);
            grpBag.Location = new Point(275, 105);
            grpBag.Size = new Size(595, 345);
            tab.Controls.Add(grpBag);

            listItems = new ListView();
            listItems.View = View.Details;
            listItems.FullRowSelect = true;
            listItems.Location = new Point(15, 25);
            listItems.Size = new Size(415, 305);
            listItems.BackColor = Color.FromArgb(32, 35, 44);
            listItems.ForeColor = Color.White;
            listItems.Columns.Add("Item Name", 260);
            listItems.Columns.Add("Quantity", 130);
            grpBag.Controls.Add(listItems);

            Button btnRemoveItem = CreateStyledButton("❌ Remove", 445, 30, 130, 34, Color.FromArgb(239, 68, 68));
            btnRemoveItem.Click += delegate { RemoveSelectedItem(); };
            grpBag.Controls.Add(btnRemoveItem);

            Button btnPlus10 = CreateStyledButton("+10 Qty", 445, 80, 130, 32, Color.FromArgb(59, 130, 246));
            btnPlus10.Click += delegate { AdjustSelectedItem(10); };
            grpBag.Controls.Add(btnPlus10);

            Button btnSet99 = CreateStyledButton("Set 99", 445, 125, 130, 32, Color.FromArgb(16, 185, 129));
            btnSet99.Click += delegate { SetSelectedItemQty(99); };
            grpBag.Controls.Add(btnSet99);
        }

        // ----------------------------------------------------
        // TAB 2: POKEMON PARTY & SPAWNER
        // ----------------------------------------------------
        private void InitPokemonTab()
        {
            TabPage tab = new TabPage("Pokemon");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            // Left Column: Party List
            GroupBox grpParty = new GroupBox();
            grpParty.Text = "Party (Max 6)";
            grpParty.ForeColor = Color.FromArgb(129, 140, 248);
            grpParty.Location = new Point(12, 10);
            grpParty.Size = new Size(265, 440);
            tab.Controls.Add(grpParty);

            listParty = new ListBox();
            listParty.Location = new Point(12, 25);
            listParty.Size = new Size(240, 260);
            listParty.BackColor = Color.FromArgb(32, 35, 44);
            listParty.ForeColor = Color.White;
            listParty.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            listParty.SelectedIndexChanged += delegate { DisplaySelectedPokemon(); };
            grpParty.Controls.Add(listParty);

            Button btnPartyShiny = CreateStyledButton("✨ Make All Shiny", 12, 300, 240, 36, Color.FromArgb(245, 158, 11));
            btnPartyShiny.Click += delegate { MakeEntirePartyShiny(); };
            grpParty.Controls.Add(btnPartyShiny);

            Button btnPartyLvl100 = CreateStyledButton("⚡ Level 100 All", 12, 345, 240, 36, Color.FromArgb(16, 185, 129));
            btnPartyLvl100.Click += delegate { Level100EntireParty(); };
            grpParty.Controls.Add(btnPartyLvl100);

            Button btnReleasePoke = CreateStyledButton("❌ Release / Delete Selected", 12, 390, 240, 36, Color.FromArgb(239, 68, 68));
            btnReleasePoke.Click += delegate { ReleaseSelectedPokemon(); };
            grpParty.Controls.Add(btnReleasePoke);

            // Middle Column: Edit Selected Pokemon (Clean, no attack move textboxes!)
            GroupBox grpDetails = new GroupBox();
            grpDetails.Text = "Edit Selected Pokémon";
            grpDetails.ForeColor = Color.FromArgb(129, 140, 248);
            grpDetails.Location = new Point(285, 10);
            grpDetails.Size = new Size(280, 440);
            tab.Controls.Add(grpDetails);

            Label lblNick = new Label();
            lblNick.Text = "Nickname:";
            lblNick.Location = new Point(12, 30);
            lblNick.AutoSize = true;
            grpDetails.Controls.Add(lblNick);

            txtPokeNick = new TextBox();
            txtPokeNick.Location = new Point(90, 26);
            txtPokeNick.Width = 175;
            txtPokeNick.BackColor = Color.FromArgb(45, 49, 60);
            txtPokeNick.ForeColor = Color.White;
            grpDetails.Controls.Add(txtPokeNick);

            Label lblLvl = new Label();
            lblLvl.Text = "Level:";
            lblLvl.Location = new Point(12, 75);
            lblLvl.AutoSize = true;
            grpDetails.Controls.Add(lblLvl);

            numPokeLevel = new NumericUpDown();
            numPokeLevel.Location = new Point(90, 72);
            numPokeLevel.Width = 65;
            numPokeLevel.Minimum = 1;
            numPokeLevel.Maximum = 100;
            numPokeLevel.BackColor = Color.FromArgb(45, 49, 60);
            numPokeLevel.ForeColor = Color.White;
            grpDetails.Controls.Add(numPokeLevel);

            Button btnLvl100 = CreateStyledButton("Max 100", 165, 71, 100, 26, Color.FromArgb(16, 185, 129));
            btnLvl100.Click += delegate { numPokeLevel.Value = 100; };
            grpDetails.Controls.Add(btnLvl100);

            Label lblHp = new Label();
            lblHp.Text = "HP:";
            lblHp.Location = new Point(12, 120);
            lblHp.AutoSize = true;
            grpDetails.Controls.Add(lblHp);

            numPokeHp = new NumericUpDown();
            numPokeHp.Location = new Point(90, 118);
            numPokeHp.Width = 65;
            numPokeHp.Minimum = 1;
            numPokeHp.Maximum = 999;
            numPokeHp.Value = 100;
            numPokeHp.BackColor = Color.FromArgb(45, 49, 60);
            numPokeHp.ForeColor = Color.White;
            grpDetails.Controls.Add(numPokeHp);

            Button btnFullHeal = CreateStyledButton("Full Heal", 165, 116, 100, 26, Color.FromArgb(59, 130, 246));
            btnFullHeal.Click += delegate {
                int lvl = (int)numPokeLevel.Value;
                numPokeHp.Value = Math.Max(50, lvl * 4 + 30);
            };
            grpDetails.Controls.Add(btnFullHeal);

            Label lblFriend = new Label();
            lblFriend.Text = "Friendship:";
            lblFriend.Location = new Point(12, 165);
            lblFriend.AutoSize = true;
            grpDetails.Controls.Add(lblFriend);

            numPokeFriend = new NumericUpDown();
            numPokeFriend.Location = new Point(90, 162);
            numPokeFriend.Width = 65;
            numPokeFriend.Minimum = 0;
            numPokeFriend.Maximum = 255;
            numPokeFriend.BackColor = Color.FromArgb(45, 49, 60);
            numPokeFriend.ForeColor = Color.White;
            grpDetails.Controls.Add(numPokeFriend);

            Button btnMaxFriend = CreateStyledButton("Max (255)", 165, 161, 100, 26, Color.FromArgb(236, 72, 153));
            btnMaxFriend.Click += delegate { numPokeFriend.Value = 255; };
            grpDetails.Controls.Add(btnMaxFriend);

            chkPokeShiny = new CheckBox();
            chkPokeShiny.Text = "✨ Is Shiny Pokémon";
            chkPokeShiny.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            chkPokeShiny.ForeColor = Color.FromArgb(251, 191, 36);
            chkPokeShiny.Location = new Point(12, 210);
            chkPokeShiny.AutoSize = true;
            grpDetails.Controls.Add(chkPokeShiny);

            Label lblNoteNatural = new Label();
            lblNoteNatural.Text = "🛡️ Moves Note:\nAttacks are kept 100% natural and learn automatically as your Pokémon battles and levels up!";
            lblNoteNatural.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            lblNoteNatural.ForeColor = Color.FromArgb(156, 163, 175);
            lblNoteNatural.Location = new Point(12, 255);
            lblNoteNatural.Size = new Size(255, 60);
            grpDetails.Controls.Add(lblNoteNatural);

            Button btnSavePoke = CreateStyledButton("💾 Save Changes", 12, 350, 255, 45, Color.FromArgb(16, 185, 129));
            btnSavePoke.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnSavePoke.Click += delegate { SaveSelectedPokemonDetails(); };
            grpDetails.Controls.Add(btnSavePoke);

            // Right Column: Spawn & Add Pokemon (900+ Available)
            GroupBox grpSpawn = new GroupBox();
            grpSpawn.Text = "➕ Spawn / Add Pokémon (900+ Available)";
            grpSpawn.ForeColor = Color.FromArgb(52, 211, 153);
            grpSpawn.Location = new Point(575, 10);
            grpSpawn.Size = new Size(300, 440);
            tab.Controls.Add(grpSpawn);

            Label lblChoose = new Label();
            lblChoose.Text = "Search Pokémon Name:";
            lblChoose.Location = new Point(12, 25);
            lblChoose.AutoSize = true;
            grpSpawn.Controls.Add(lblChoose);

            cmbSpawnPoke = new ComboBox();
            cmbSpawnPoke.Location = new Point(12, 50);
            cmbSpawnPoke.Width = 270;
            cmbSpawnPoke.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSpawnPoke.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbSpawnPoke.BackColor = Color.FromArgb(45, 49, 60);
            cmbSpawnPoke.ForeColor = Color.White;
            foreach (string pName in allPokemonNames) cmbSpawnPoke.Items.Add(pName);
            if (cmbSpawnPoke.Items.Count > 0) cmbSpawnPoke.SelectedIndex = 0;
            grpSpawn.Controls.Add(cmbSpawnPoke);

            Label lblSpawnLvl = new Label();
            lblSpawnLvl.Text = "Spawn Level:";
            lblSpawnLvl.Location = new Point(12, 90);
            lblSpawnLvl.AutoSize = true;
            grpSpawn.Controls.Add(lblSpawnLvl);

            numSpawnLevel = new NumericUpDown();
            numSpawnLevel.Location = new Point(105, 88);
            numSpawnLevel.Width = 65;
            numSpawnLevel.Minimum = 1;
            numSpawnLevel.Maximum = 100;
            numSpawnLevel.Value = 50;
            numSpawnLevel.BackColor = Color.FromArgb(45, 49, 60);
            numSpawnLevel.ForeColor = Color.White;
            grpSpawn.Controls.Add(numSpawnLevel);

            Button btnSpawnLvl100 = CreateStyledButton("Max 100", 180, 86, 100, 26, Color.FromArgb(16, 185, 129));
            btnSpawnLvl100.Click += delegate { numSpawnLevel.Value = 100; };
            grpSpawn.Controls.Add(btnSpawnLvl100);

            Label lblGender = new Label();
            lblGender.Text = "Gender:";
            lblGender.Location = new Point(12, 130);
            lblGender.AutoSize = true;
            grpSpawn.Controls.Add(lblGender);

            cmbSpawnGender = new ComboBox();
            cmbSpawnGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpawnGender.Location = new Point(105, 126);
            cmbSpawnGender.Width = 175;
            cmbSpawnGender.BackColor = Color.FromArgb(45, 49, 60);
            cmbSpawnGender.ForeColor = Color.White;
            cmbSpawnGender.Items.AddRange(new string[] { "male", "female", "genderless" });
            cmbSpawnGender.SelectedIndex = 0;
            grpSpawn.Controls.Add(cmbSpawnGender);

            Label lblCustomNick = new Label();
            lblCustomNick.Text = "Nickname:";
            lblCustomNick.Location = new Point(12, 170);
            lblCustomNick.AutoSize = true;
            grpSpawn.Controls.Add(lblCustomNick);

            txtSpawnNick = new TextBox();
            txtSpawnNick.Location = new Point(105, 166);
            txtSpawnNick.Width = 175;
            txtSpawnNick.BackColor = Color.FromArgb(45, 49, 60);
            txtSpawnNick.ForeColor = Color.White;
            grpSpawn.Controls.Add(txtSpawnNick);

            chkSpawnShiny = new CheckBox();
            chkSpawnShiny.Text = "✨ Spawn as Shiny Pokémon!";
            chkSpawnShiny.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            chkSpawnShiny.ForeColor = Color.FromArgb(251, 191, 36);
            chkSpawnShiny.Location = new Point(12, 205);
            chkSpawnShiny.AutoSize = true;
            grpSpawn.Controls.Add(chkSpawnShiny);

            Button btnAddSpawn = CreateStyledButton("➕ Add to Party (Slot)", 12, 240, 275, 42, Color.FromArgb(16, 185, 129));
            btnAddSpawn.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnAddSpawn.Click += delegate {
                if (cmbSpawnPoke.SelectedItem != null)
                {
                    string p = cmbSpawnPoke.SelectedItem.ToString();
                    int lvl = (int)numSpawnLevel.Value;
                    bool isSh = chkSpawnShiny.Checked;
                    string gen = cmbSpawnGender.SelectedItem != null ? cmbSpawnGender.SelectedItem.ToString() : "male";
                    string nk = txtSpawnNick.Text.Trim();
                    AddNewPokemonToParty(p, lvl, isSh, gen, nk);
                }
            };
            grpSpawn.Controls.Add(btnAddSpawn);

            // Preset fast spawners
            Label lblPresets = new Label();
            lblPresets.Text = "Quick Preset Spawners:";
            lblPresets.ForeColor = Color.FromArgb(156, 163, 175);
            lblPresets.Location = new Point(12, 290);
            lblPresets.AutoSize = true;
            grpSpawn.Controls.Add(lblPresets);

            Button btnPresetMewtwo = CreateStyledButton("★ Shiny Mewtwo (Lvl 100)", 12, 315, 275, 26, Color.FromArgb(147, 51, 234));
            btnPresetMewtwo.Click += delegate { AddNewPokemonToParty("mewtwo", 100, true, "genderless", "mewtwo"); };
            grpSpawn.Controls.Add(btnPresetMewtwo);

            Button btnPresetCharizard = CreateStyledButton("🔥 Shiny Charizard (Lvl 100)", 12, 345, 275, 26, Color.FromArgb(239, 68, 68));
            btnPresetCharizard.Click += delegate { AddNewPokemonToParty("charizard", 100, true, "male", "charizard"); };
            grpSpawn.Controls.Add(btnPresetCharizard);

            Button btnPresetRayquaza = CreateStyledButton("🐉 Shiny Rayquaza (Lvl 100)", 12, 375, 275, 26, Color.FromArgb(16, 185, 129));
            btnPresetRayquaza.Click += delegate { AddNewPokemonToParty("rayquaza", 100, true, "genderless", "rayquaza"); };
            grpSpawn.Controls.Add(btnPresetRayquaza);

            Button btnPresetGengar = CreateStyledButton("👻 Shiny Gengar (Lvl 100)", 12, 405, 275, 26, Color.FromArgb(139, 92, 246));
            btnPresetGengar.Click += delegate { AddNewPokemonToParty("gengar", 100, true, "male", "gengar"); };
            grpSpawn.Controls.Add(btnPresetGengar);
        }

        // ----------------------------------------------------
        // TAB 3: WORLD CHEATS & BACKUPS
        // ----------------------------------------------------
        private void InitWorldTab()
        {
            TabPage tab = new TabPage("World");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            GroupBox grpTime = new GroupBox();
            grpTime.Text = "Time of Day";
            grpTime.ForeColor = Color.FromArgb(129, 140, 248);
            grpTime.Location = new Point(20, 20);
            grpTime.Size = new Size(350, 160);
            tab.Controls.Add(grpTime);

            radDay = new RadioButton();
            radDay.Text = "☀️ Day";
            radDay.Location = new Point(25, 35);
            radDay.Checked = true;
            grpTime.Controls.Add(radDay);

            radNight = new RadioButton();
            radNight.Text = "🌙 Night (Ghost/Dark Spawns)";
            radNight.Location = new Point(25, 75);
            grpTime.Controls.Add(radNight);

            radDusk = new RadioButton();
            radDusk.Text = "🌅 Dusk / Sunset";
            radDusk.Location = new Point(25, 115);
            grpTime.Controls.Add(radDusk);

            Button btnApplyTime = CreateStyledButton("Apply Time of Day", 390, 50, 180, 36, Color.FromArgb(245, 158, 11));
            btnApplyTime.Click += delegate {
                if (saveData != null) {
                    if (radDay.Checked) saveData["timeOfDay"] = "day";
                    else if (radNight.Checked) saveData["timeOfDay"] = "night";
                    else if (radDusk.Checked) saveData["timeOfDay"] = "dusk";
                    ApplyAndSave("Time of day set to " + saveData["timeOfDay"]);
                }
            };
            tab.Controls.Add(btnApplyTime);

            GroupBox grpBackup = new GroupBox();
            grpBackup.Text = "Save Backups & Safety";
            grpBackup.ForeColor = Color.FromArgb(129, 140, 248);
            grpBackup.Location = new Point(20, 200);
            grpBackup.Size = new Size(550, 120);
            tab.Controls.Add(grpBackup);

            Label lblBackupInfo = new Label();
            lblBackupInfo.Text = "Before any cheat is applied, an automatic backup (.bak) is generated in your .sav folder. If you ever want to restore your original save:";
            lblBackupInfo.Location = new Point(15, 25);
            lblBackupInfo.Size = new Size(520, 40);
            grpBackup.Controls.Add(lblBackupInfo);

            Button btnRestore = CreateStyledButton("⏮ Restore from .bak Backup", 15, 70, 220, 34, Color.FromArgb(239, 68, 68));
            btnRestore.Click += delegate { RestoreBackup(); };
            grpBackup.Controls.Add(btnRestore);
        }

        // ----------------------------------------------------
        // DATA LOGIC & SAVE HANDLING
        // ----------------------------------------------------
        private void ScanSaves()
        {
            string prevSelected = cmbSaves.SelectedItem != null ? cmbSaves.SelectedItem.ToString() : "";
            cmbSaves.Items.Clear();
            if (!Directory.Exists(gameDir)) return;

            string[] dirs = Directory.GetDirectories(gameDir, "*.sav");
            foreach (string d in dirs)
            {
                cmbSaves.Items.Add(Path.GetFileName(d));
            }

            if (cmbSaves.Items.Count > 0)
            {
                int matchIdx = -1;
                if (!string.IsNullOrEmpty(prevSelected))
                {
                    for (int i = 0; i < cmbSaves.Items.Count; i++)
                    {
                        if (cmbSaves.Items[i].ToString().Equals(prevSelected, StringComparison.OrdinalIgnoreCase))
                        {
                            matchIdx = i;
                            break;
                        }
                    }
                }
                cmbSaves.SelectedIndex = matchIdx >= 0 ? matchIdx : 0;
            }
            else
            {
                lblPlayerInfo.Text = "No .sav folder found! Please launch game & click Save once.";
            }
        }

        private void LoadSelectedSave()
        {
            if (cmbSaves.SelectedItem == null) return;
            activeSavFolder = Path.Combine(gameDir, cmbSaves.SelectedItem.ToString());
            activeZipPath = Path.Combine(activeSavFolder, "game.json.zip");

            if (!File.Exists(activeZipPath))
            {
                lblPlayerInfo.Text = "game.json.zip missing in " + cmbSaves.SelectedItem;
                return;
            }

            try
            {
                string json = "";
                using (var zip = ZipFile.OpenRead(activeZipPath))
                {
                    var entry = zip.GetEntry("data.json");
                    if (entry == null) entry = zip.GetEntry("game.json");
                    if (entry != null)
                    {
                        using (var reader = new StreamReader(entry.Open()))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                }

                saveData = jsonSer.Deserialize<Dictionary<string, object>>(json);
                RefreshUIFromData();
                lblStatus.Text = "Loaded: " + cmbSaves.SelectedItem + " successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load save: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshUIFromData()
        {
            if (saveData == null) return;

            Dictionary<string, object> player = saveData["playerData"] as Dictionary<string, object>;
            if (player != null)
            {
                string pName = player.ContainsKey("name") ? player["name"].ToString() : "Player";
                string map = saveData.ContainsKey("currMapId") ? saveData["currMapId"].ToString() : "0,0";
                lblPlayerInfo.Text = string.Format("Player: {0} | Map: {1} | World: {2}", pName, map, Path.GetFileName(activeSavFolder));

                // Items list
                listItems.Items.Clear();
                if (player.ContainsKey("itemsDict"))
                {
                    Dictionary<string, object> items = player["itemsDict"] as Dictionary<string, object>;
                    if (items != null)
                    {
                        foreach (var kv in items)
                        {
                            ListViewItem lvi = new ListViewItem(kv.Key);
                            lvi.SubItems.Add(kv.Value.ToString());
                            listItems.Items.Add(lvi);
                        }
                    }
                }

                // Party list (Support IList for ArrayList from JavaScriptSerializer!)
                int prevSelected = listParty.SelectedIndex;
                listParty.Items.Clear();
                if (player.ContainsKey("pokemon"))
                {
                    IList pokeList = player["pokemon"] as IList;
                    if (pokeList != null)
                    {
                        for (int i = 0; i < pokeList.Count; i++)
                        {
                            var pDict = pokeList[i] as Dictionary<string, object>;
                            if (pDict != null)
                            {
                                string pNameStr = pDict.ContainsKey("nickname") ? pDict["nickname"].ToString() : (pDict.ContainsKey("name") ? pDict["name"].ToString() : "Pokemon");
                                string pLvl = pDict.ContainsKey("level") ? pDict["level"].ToString() : "1";
                                bool isShiny = pDict.ContainsKey("isShiny") && Convert.ToBoolean(pDict["isShiny"]);
                                string shinyBadge = isShiny ? "✨ " : "";
                                listParty.Items.Add(string.Format("{0}. {1}{2} (Lvl {3})", i + 1, shinyBadge, pNameStr, pLvl));
                            }
                        }
                    }
                }

                if (listParty.Items.Count > 0)
                {
                    listParty.SelectedIndex = (prevSelected >= 0 && prevSelected < listParty.Items.Count) ? prevSelected : 0;
                }
                else
                {
                    txtPokeNick.Clear();
                    numPokeLevel.Value = 1;
                    numPokeHp.Value = 20;
                    numPokeFriend.Value = 70;
                    chkPokeShiny.Checked = false;
                }

                // Time of day
                if (saveData.ContainsKey("timeOfDay"))
                {
                    string tod = saveData["timeOfDay"].ToString().ToLower();
                    if (tod == "night") radNight.Checked = true;
                    else if (tod == "dusk") radDusk.Checked = true;
                    else radDay.Checked = true;
                }
            }
        }

        private void DisplaySelectedPokemon()
        {
            if (listParty.SelectedIndex < 0 || saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as IList;
            if (pokeList == null || listParty.SelectedIndex >= pokeList.Count) return;

            var p = pokeList[listParty.SelectedIndex] as Dictionary<string, object>;
            if (p == null) return;

            txtPokeNick.Text = p.ContainsKey("nickname") ? p["nickname"].ToString() : (p.ContainsKey("name") ? p["name"].ToString() : "");
            numPokeLevel.Value = p.ContainsKey("level") ? Math.Min(100, Math.Max(1, Convert.ToInt32(p["level"]))) : 1;
            numPokeHp.Value = p.ContainsKey("hp") ? Math.Min(999, Math.Max(1, Convert.ToInt32(p["hp"]))) : 20;
            numPokeFriend.Value = p.ContainsKey("friendliness") ? Math.Min(255, Math.Max(0, Convert.ToInt32(p["friendliness"]))) : 70;
            chkPokeShiny.Checked = p.ContainsKey("isShiny") && Convert.ToBoolean(p["isShiny"]);
        }

        private void SaveSelectedPokemonDetails()
        {
            if (listParty.SelectedIndex < 0 || saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as IList;
            if (pokeList == null || listParty.SelectedIndex >= pokeList.Count) return;

            var p = pokeList[listParty.SelectedIndex] as Dictionary<string, object>;
            if (p == null) return;

            string newNick = txtPokeNick.Text.Trim();
            if (!string.IsNullOrEmpty(newNick)) p["nickname"] = newNick;

            int newLvl = (int)numPokeLevel.Value;
            p["level"] = newLvl;

            // FIX: Re-calculate EXP properly so game doesn't level-up in an endless loop!
            if (newLvl <= 1)
            {
                p["exp"] = 0;
            }
            else
            {
                // Pokemon Medium-Slow curve: 1.2*n^3 - 15*n^2 + 100*n - 140
                int calculatedExp = (int)Math.Max(0, (1.2 * newLvl * newLvl * newLvl) - (15.0 * newLvl * newLvl) + (100.0 * newLvl) - 140);
                p["exp"] = calculatedExp;
            }

            p["hp"] = (int)numPokeHp.Value;
            p["friendliness"] = (int)numPokeFriend.Value;
            p["isShiny"] = chkPokeShiny.Checked;

            // Note: attacks are kept untouched to preserve natural moves!

            // Also update currPokemon if it is slot 0
            if (listParty.SelectedIndex == 0 && player.ContainsKey("currPokemon"))
            {
                player["currPokemon"] = p;
            }

            ApplyAndSave("Updated " + txtPokeNick.Text);
            RefreshUIFromData();
        }

        // ----------------------------------------------------
        // POKEMON SPAWNER & MANAGEMENT
        // ----------------------------------------------------
        public void AddNewPokemonToParty(string pokeName, int level, bool isShiny, string gender, string nick)
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;

            List<object> list = new List<object>();
            if (player.ContainsKey("pokemon") && player["pokemon"] is IList)
            {
                foreach (var item in (IList)player["pokemon"])
                {
                    list.Add(item);
                }
            }

            if (list.Count >= 6)
            {
                MessageBox.Show("Your Party is FULL (6/6 Pokémon)!\n\nPlease release/delete a Pokémon from your party first using the 'Release / Delete Selected' button.", "Party Full", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cleanName = pokeName.Trim().ToLower();
            string cleanNick = string.IsNullOrEmpty(nick) ? cleanName : nick.Trim();
            int hpVal = Math.Max(40, level * 3 + 25);
            
            // Proper EXP formula for the spawned level
            int expVal = level <= 1 ? 0 : (int)Math.Max(0, (1.2 * level * level * level) - (15.0 * level * level) + (100.0 * level) - 140);

            // Natural default moves for this species and level
            List<string> naturalMoves = GetDefaultMovesForSpecies(cleanName, level);

            Dictionary<string, object> newPoke = new Dictionary<string, object>();
            newPoke["name"] = cleanName;
            newPoke["nickname"] = cleanNick;
            newPoke["level"] = level;
            newPoke["hp"] = hpVal;
            newPoke["exp"] = expVal;
            newPoke["gender"] = string.IsNullOrEmpty(gender) ? "male" : gender.ToLower();
            newPoke["friendliness"] = 150;
            newPoke["aggroPlayer"] = false;
            newPoke["test"] = false;
            newPoke["generation"] = "CRYSTAL";
            newPoke["isShiny"] = isShiny;
            newPoke["attacks"] = naturalMoves.ToArray();
            newPoke["index"] = list.Count;
            newPoke["previousOwnerName"] = player.ContainsKey("name") ? player["name"].ToString() : "Player";
            newPoke["position"] = 0;
            newPoke["interiorIndex"] = 100;
            newPoke["isInterior"] = false;
            newPoke["harvestTimer"] = 0;

            list.Add(newPoke);
            player["pokemon"] = list.ToArray();

            if (list.Count == 1 || !player.ContainsKey("currPokemon"))
            {
                player["currPokemon"] = newPoke;
            }

            string shinyText = isShiny ? "✨ Shiny " : "";
            ApplyAndSave(string.Format("Added {0}{1} (Lvl {2}) to Party!", shinyText, cleanNick, level));
            RefreshUIFromData();
            listParty.SelectedIndex = list.Count - 1;
        }

        private void ReleaseSelectedPokemon()
        {
            if (listParty.SelectedIndex < 0 || saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;

            List<object> list = new List<object>();
            if (player.ContainsKey("pokemon") && player["pokemon"] is IList)
            {
                foreach (var item in (IList)player["pokemon"])
                {
                    list.Add(item);
                }
            }

            if (list.Count <= 1)
            {
                MessageBox.Show("You cannot release your last Pokémon! You must keep at least 1 Pokémon in your party.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selIdx = listParty.SelectedIndex;
            var pDict = list[selIdx] as Dictionary<string, object>;
            string pName = pDict != null && pDict.ContainsKey("nickname") ? pDict["nickname"].ToString() : "Pokemon";

            DialogResult res = MessageBox.Show(string.Format("Are you sure you want to release {0} from your party?", pName), "Confirm Release", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            list.RemoveAt(selIdx);
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i] as Dictionary<string, object>;
                if (p != null) p["index"] = i;
            }
            player["pokemon"] = list.ToArray();

            if (player.ContainsKey("currPokemon"))
            {
                player["currPokemon"] = list[0];
            }

            ApplyAndSave(string.Format("Released {0} from party.", pName));
            RefreshUIFromData();
        }

        private void MakeEntirePartyShiny()
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as IList;
            if (pokeList == null) return;

            foreach (var pObj in pokeList)
            {
                var p = pObj as Dictionary<string, object>;
                if (p != null) p["isShiny"] = true;
            }
            if (player.ContainsKey("currPokemon") && player["currPokemon"] is Dictionary<string, object>)
            {
                ((Dictionary<string, object>)player["currPokemon"])["isShiny"] = true;
            }

            ApplyAndSave("All Party Pokémon are now SHINY! ✨");
            RefreshUIFromData();
        }

        private void Level100EntireParty()
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as IList;
            if (pokeList == null) return;

            foreach (var pObj in pokeList)
            {
                var p = pObj as Dictionary<string, object>;
                if (p != null)
                {
                    p["level"] = 100;
                    p["hp"] = 350;
                    p["exp"] = 1000000;
                }
            }
            ApplyAndSave("All Party Pokémon set to Level 100! ⚡");
            RefreshUIFromData();
        }

        private void AddItemDirectly(string itemName, int count)
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;

            Dictionary<string, object> items;
            if (!player.ContainsKey("itemsDict") || !(player["itemsDict"] is Dictionary<string, object>))
            {
                items = new Dictionary<string, object>();
                player["itemsDict"] = items;
            }
            else
            {
                items = (Dictionary<string, object>)player["itemsDict"];
            }

            if (items.ContainsKey(itemName))
            {
                items[itemName] = Convert.ToInt32(items[itemName]) + count;
            }
            else
            {
                items[itemName] = count;
            }

            ApplyAndSave(string.Format("+{0} {1} added to bag!", count, itemName));
            RefreshUIFromData();
        }

        private void AddAllEvoStones(int count)
        {
            string[] stones = new string[] {
                "fire stone", "water stone", "thunderstone", "leaf stone", "moon stone", 
                "sun stone", "dawn stone", "dusk stone", "shiny stone", "ice stone", "hard stone"
            };
            foreach (string s in stones) AddItemDirectly(s, count);
        }

        private void AddBerriesAndRopes(int count)
        {
            string[] items = new string[] {
                "escape rope", "lum berry", "pecha berry", "cheri berry", "aspear berry", "chesto berry", "rawst berry", "persim berry"
            };
            foreach (string itm in items) AddItemDirectly(itm, count);
        }

        private void AddAllPokeballs(int count)
        {
            string[] balls = new string[] {
                "master ball", "ultra ball", "dusk ball", "heavy ball", "moon ball", "fast ball", "love ball", "level ball", "net ball"
            };
            foreach (string b in balls) AddItemDirectly(b, count);
        }

        private void MaxOutAllItems()
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null || !player.ContainsKey("itemsDict")) return;
            var items = player["itemsDict"] as Dictionary<string, object>;
            if (items == null) return;

            List<string> keys = new List<string>(items.Keys);
            foreach (string k in keys) items[k] = 999;

            ApplyAndSave("Maxed out all items in bag to 999!");
            RefreshUIFromData();
        }

        private void RemoveSelectedItem()
        {
            if (listItems.SelectedItems.Count == 0 || saveData == null) return;
            string itm = listItems.SelectedItems[0].Text;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var items = player["itemsDict"] as Dictionary<string, object>;
            if (items != null && items.ContainsKey(itm))
            {
                items.Remove(itm);
                ApplyAndSave("Removed " + itm);
                RefreshUIFromData();
            }
        }

        private void AdjustSelectedItem(int amount)
        {
            if (listItems.SelectedItems.Count == 0 || saveData == null) return;
            string itm = listItems.SelectedItems[0].Text;
            AddItemDirectly(itm, amount);
        }

        private void SetSelectedItemQty(int target)
        {
            if (listItems.SelectedItems.Count == 0 || saveData == null) return;
            string itm = listItems.SelectedItems[0].Text;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var items = player["itemsDict"] as Dictionary<string, object>;
            if (items != null)
            {
                items[itm] = target;
                ApplyAndSave(string.Format("Set {0} to {1}", itm, target));
                RefreshUIFromData();
            }
        }

        private void RestoreBackup()
        {
            if (string.IsNullOrEmpty(activeZipPath)) return;
            string bakPath = activeZipPath + ".bak";
            if (!File.Exists(bakPath))
            {
                MessageBox.Show("No .bak backup found for this world!", "Backup Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult res = MessageBox.Show("Restore save from original .bak backup? Current changes will be overwritten.", "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                try
                {
                    File.Copy(bakPath, activeZipPath, true);
                    LoadSelectedSave();
                    MessageBox.Show("Save restored from backup successfully!", "Restored", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Restore failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyAndSave(string statusMsg)
        {
            if (saveData == null || string.IsNullOrEmpty(activeZipPath)) return;

            try
            {
                // Ensure backup exists
                string bakPath = activeZipPath + ".bak";
                if (!File.Exists(bakPath) && File.Exists(activeZipPath))
                {
                    File.Copy(activeZipPath, bakPath, true);
                }

                string newJson = jsonSer.Serialize(saveData);

                // Write to temp zip then atomic replace
                string tempZip = activeZipPath + ".tmp";
                if (File.Exists(tempZip)) File.Delete(tempZip);

                using (var zip = ZipFile.Open(tempZip, ZipArchiveMode.Create))
                {
                    var entry = zip.CreateEntry("data.json", CompressionLevel.Optimal);
                    using (var writer = new StreamWriter(entry.Open()))
                    {
                        writer.Write(newJson);
                    }
                }

                if (File.Exists(activeZipPath)) File.Delete(activeZipPath);
                File.Move(tempZip, activeZipPath);

                lblStatus.Text = string.Format("{0} (Saved at {1:HH:mm:ss})", statusMsg, DateTime.Now);
                lblStatus.ForeColor = Color.FromArgb(52, 211, 153);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to game: " + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Save failed: " + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
            }
        }
    }
}
