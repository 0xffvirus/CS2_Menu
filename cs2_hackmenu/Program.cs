using Swed64;
using ClickableTransparentOverlay;
using System.Numerics;
using cs2_hackmenu;
using System.Threading;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace cs2_cheat
{

    class Program : Overlay
    {

        //imports 

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vkey); // key listener


        [DllImport("user32.dll")]

        static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public RECT GetWindowRect(IntPtr hWnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hWnd, out rect);
            return rect;
        }




        Swed swed = new Swed("cs2"); 
        Offsets offsets = new Offsets(); // load offsets
        ImDrawListPtr drawList;
        Entity localplayer = new Entity(); // load entity for localplayer
        List<Entity> entities = new List<Entity>(); // load all entities for all players
        List<Entity> enemyTeam = new List<Entity>(); 
        List<Entity> playerTeam = new List<Entity>();

        // constants
        const int AIMBOT_HOTKEY = 0x06; // xbutton2, mouse 5 virtual key code

        // other vectors
        Vector3 offsetVector = new Vector3(0, 0, 10); // substract 10 units from the height of the character

        // IMGui Colors
        Vector4 teamcolor = new Vector4(0, 0, 1, 1); // RGBA blue color
        Vector4 enemycolor = new Vector4(1, 0, 0, 1); 
        Vector4 healthBarColor = new Vector4(0, 1, 0, 1); // green
        Vector4 healthTextColor = new Vector4(0, 0, 0, 1); // black

        // Screen Variables
        Vector2 windowLocation = new Vector2(0, 0);
        Vector2 windowSize = new Vector2(1920, 1080);
        Vector2 lineOrigin = new Vector2(1920 / 2, 1080); // middle of screen
        Vector2 windowCenter = new Vector2(1920 / 2, 1080 / 2);

        // ImGui Checkboxs
        bool enableESP = true;
        bool enableAimBot = true;
        bool enableAimbotCrosshair = false;

        // [Team]
        bool enableTeamLine = true;
        bool enableTeamBox = true;
        bool enableTeamDot = false;
        bool enableTeamHealthBar = true;
        bool enableTeamDistance = true;

        // [Enemy]
        bool enableEnemyLine = true;
        bool enableEnemyBox = true;
        bool enableEnemyDot = false;
        bool enableEnemyHealthBar = true;
        bool enableEnemyDistance = true;





        IntPtr client;
        protected override void Render() // for IMGui Renderer
        {
            DrawMenu();
            DrawOverlay();
            ESP();
            ImGui.End();
        }

        // [AIMBOT IS HERE] functions used [AimAt, CalculateAngles, CalculatePixelDistance, CalculateMagnitude]
        void Aimbot()
        {
            if (GetAsyncKeyState(AIMBOT_HOTKEY) < 0 && enableAimBot)
            {
                if (enemyTeam.Count > 0)
                {
                    var angles = CalculateAngles(localplayer.origin, Vector3.Subtract(enemyTeam[0].origin, offsetVector));
                    AimAt(angles);
                }
            }
        }

        void AimAt(Vector3 angles)
        {
            swed.WriteFloat(client, offsets.viewAngle, angles.Y); // Y was before X in cs2
            swed.WriteFloat(client, offsets.viewAngle + 0x4, angles.X); // a float is 4 bytes, so we skipped 4 bytes to access X axis
        }

        Vector3 CalculateAngles(Vector3 from, Vector3 destination)
        {
            float yaw;
            float pitch;

            // calculate yaw

            float DeltaX = destination.X - from.X;
            float DeltaY = destination.Y - from.Y;
            yaw = (float)(Math.Atan2(DeltaY, DeltaX) * 180 / Math.PI); // get angle from tan triangle 


            // calculate pitch
            float DeltaZ = destination.Z - from.Z;
            double distance = Math.Sqrt(Math.Pow(DeltaX, 2) + Math.Pow(DeltaY, 2)); // c = Math.Sqrt(a^2+b^2) 
            pitch = -(float)(Math.Atan2(DeltaZ, distance) * 180 / Math.PI);

            // return angles
            return new Vector3(yaw, pitch, 0);
        }

        float CalculatePixelDistance(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y, 2));
        }

        float CalculateMagnitude(Vector3 v1, Vector3 v2)
        {
            return (float)Math.Sqrt(Math.Pow(v2.X - v1.X, 2) + Math.Pow(v2.Y - v1.Y,2) + Math.Pow(v2.Z - v1.Z,2)); // Distance Formula (Delta Values)
        }

        // [Everything belongs to ESP] functions used [DrawVisuals, ReadviewMatrix, WorldToScreen, isPixelInSideScreen]
        void ESP()
        {
            drawList = ImGui.GetWindowDrawList();

            if (enableESP)
            {
                try
                {
                    foreach (var entity in entities)
                    {
                        if (entity.teamNum == localplayer.teamNum)
                        {
                            DrawVisuals(entity, teamcolor, enableTeamLine, enableTeamBox, enableTeamDot, enableTeamHealthBar, enableTeamDistance);
                        } else
                        {
                            DrawVisuals(entity, enemycolor, enableEnemyLine, enableEnemyBox, enableEnemyDot, enableEnemyHealthBar, enableEnemyDistance);
                        }
                    }
                } 
                catch (Exception e)
                 {
                    Console.WriteLine(e);
                }
            }
        }

        viewMatrix ReadviewMatrix(IntPtr MatrixAddress)
        {
            var viewmatrix = new viewMatrix();
            var floatMatrix = swed.ReadMatrix(MatrixAddress);

            viewmatrix.m11 = floatMatrix[0];
            viewmatrix.m12 = floatMatrix[1];
            viewmatrix.m13 = floatMatrix[2];
            viewmatrix.m14 = floatMatrix[3];

            viewmatrix.m21 = floatMatrix[4];
            viewmatrix.m22 = floatMatrix[5];
            viewmatrix.m23 = floatMatrix[6];
            viewmatrix.m24 = floatMatrix[7];

            viewmatrix.m31 = floatMatrix[8];
            viewmatrix.m32 = floatMatrix[9];
            viewmatrix.m33 = floatMatrix[10];
            viewmatrix.m34 = floatMatrix[11];

            viewmatrix.m41 = floatMatrix[12];
            viewmatrix.m42 = floatMatrix[13];
            viewmatrix.m43 = floatMatrix[14];
            viewmatrix.m44 = floatMatrix[15];

            return viewmatrix;
        }
        
        void DrawVisuals(Entity entity, Vector4 color, bool line, bool box, bool dot, bool Healthbar, bool distance)
        {
            // check if 2d position is valid
            if (isPixelInSideScreen(entity.ScreenPosition))
            {
                // convert colors to uint
                uint uintColor = ImGui.ColorConvertFloat4ToU32(color);
                uint uintHealthTextColor = ImGui.ColorConvertFloat4ToU32(healthTextColor);
                uint uintHealthBarColor = ImGui.ColorConvertFloat4ToU32(healthBarColor);

                // calculate box attributes

                Vector2 boxWidth = new Vector2((entity.ScreenPosition.Y - entity.absScreenPosition.Y) / 2, 0f); // divide height by 2 to simulate width
                Vector2 BoxStart = Vector2.Subtract(entity.absScreenPosition, boxWidth);
                Vector2 BoxEnd = Vector2.Add(entity.ScreenPosition, boxWidth);

                // calculate health bar
                float barPercent = entity.Health / 100f;
                Vector2 barHeight = new Vector2(0, barPercent * (entity.ScreenPosition.Y - entity.absScreenPosition.Y)); // multiply barPercent by player height
                Vector2 barStart = Vector2.Subtract(Vector2.Subtract(entity.ScreenPosition, boxWidth), barHeight);
                Vector2 barEnd = Vector2.Subtract(entity.ScreenPosition, Vector2.Add(boxWidth, new Vector2(-4, 0)));


                if (line )
                {
                    drawList.AddLine(lineOrigin, entity.ScreenPosition, uintColor, 2);
                } 
                if (box)
                {
                    drawList.AddRect(BoxStart, BoxEnd, uintColor, 3);
                }
                if (dot)
                {
                    drawList.AddCircleFilled(entity.ScreenPosition, 5, uintColor);
                } 
                if (Healthbar)
                {
                    drawList.AddText(entity.ScreenPosition, uintHealthTextColor, $"hp: {entity.Health}");
                    drawList.AddText(entity.ScreenPosition, uintHealthTextColor, $"\nDistance: {(int)entity.magnitude/100}");
                    drawList.AddRectFilled(barStart, barEnd, uintHealthBarColor);
                }

            }
        }

        bool isPixelInSideScreen(Vector2 pixel)  // check all window bounds
        {
            return pixel.X > windowLocation.X && pixel.X < windowLocation.X + windowSize.X && pixel.Y > windowLocation.Y && pixel.Y < windowLocation.Y + windowSize.Y;
        }
        Vector2 WorldToScreen(viewMatrix Matrix, Vector3 pos, int width, int height) 
        {
            Vector2 screenCoordinates = new Vector2();

            float screenW = (Matrix.m41 * pos.X) + (Matrix.m42 * pos.Y) + (Matrix.m43 * pos.Z) + Matrix.m44;

            if (screenW > 0.001f )
            {
                float screenX = (Matrix.m11 * pos.X) + (Matrix.m12 * pos.Y) + (Matrix.m13 * pos.Z) + Matrix.m14;

                float screenY = (Matrix.m21 * pos.X) + (Matrix.m22 * pos.Y) + (Matrix.m23 * pos.Z) + Matrix.m24;

                float camX = width / 2;
                float camY = height / 2;

                float X = camX + (camX * screenX / screenW);
                float Y = camY - (camY * screenY / screenW);


                screenCoordinates.X = X;
                screenCoordinates.Y = Y;

                return screenCoordinates;
            } else
            {
                return new Vector2(-99, -99);
            }
        }

        // [IMGUI Functions]
        void DrawMenu()
        {
            ImGui.Begin("CS2 Cheat");

            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    ImGui.Checkbox("ESP", ref enableESP);
                    ImGui.Checkbox("Aimbot", ref enableAimBot);

                    if (enableAimBot)
                    {
                        ImGui.SameLine();
                        ImGui.Checkbox("closest to crosshair", ref enableAimbotCrosshair);
                    } else
                    {
                        enableAimbotCrosshair = false;
                    }
                    ImGui.EndTabItem();
                } 
                if (ImGui.BeginTabItem("Colors"))
                {
                    // team settings
                    ImGui.ColorPicker4("Team color", ref teamcolor);
                    ImGui.Checkbox("team line", ref enableTeamLine);
                    ImGui.Checkbox("Team box", ref enableTeamBox);
                    ImGui.Checkbox("Team Dot", ref enableTeamDot);
                    ImGui.Checkbox("Team HealthBar", ref enableTeamHealthBar);

                    // enemy settings
                    ImGui.ColorPicker4("Enemy color", ref enemycolor);
                    ImGui.Checkbox("Enemy line", ref enableEnemyLine);
                    ImGui.Checkbox("Enemy box", ref enableEnemyBox);
                    ImGui.Checkbox("Enemy Dot", ref enableEnemyDot);
                    ImGui.Checkbox("Enemy HealthBar", ref enableEnemyHealthBar);
                    ImGui.EndTabItem();

                }
            }
            ImGui.EndTabBar();
        }

        void DrawOverlay() // draw an overlay for game
        {
            ImGui.SetNextWindowSize(windowSize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse);
        }


        // [Reading Data and Logic Functions]
        void MainLogic()
        {

            // calc window sizes and position
            var window = GetWindowRect(swed.GetProcess().MainWindowHandle);
            windowLocation = new Vector2(window.left, window.top); // get window locaiton in screen
            windowSize = Vector2.Subtract(new Vector2(window.right, window.bottom), windowLocation); // get window size 
            lineOrigin = new Vector2(windowLocation.X + windowSize.X / 2, window.bottom); 
            windowCenter = new Vector2(lineOrigin.X, window.bottom - windowSize.Y / 2);


            client = swed.GetModuleBase("client.dll"); // get access to modulebase [Process]

            while (true) // always run
            {

                ReloadEntities();
                
                if(enableAimBot)
                {
                    Aimbot();
                }

                Thread.Sleep(3);
                int i = 0;
            }
        }

        void ReloadEntities()
        {
            entities.Clear();
            playerTeam.Clear();
            enemyTeam.Clear();

            localplayer.address = swed.ReadPointer(client, offsets.localplayer); // read localplayer address from the pointer

            UpdateEntity(localplayer); // get all localplayer information (such as health, origin)
            UpdateEntities(); // get all players information (such as health, origin)

            enemyTeam = enemyTeam.OrderBy(o => o.magnitude).ToList();

            // order enemies in pixel difference 
            if (enableAimbotCrosshair)
            {
                enemyTeam = enemyTeam.OrderBy(o => o.angleDifference).ToList();
            } 

        }

        void UpdateEntities()
        {
            for (int i = 0; i < 64; i++) 
            {
                IntPtr tempEntityAddress = swed.ReadPointer(client, offsets.entitylist + i * 0x08); // read all players addresses
                if (tempEntityAddress == IntPtr.Zero) // skip if invalid
                    continue;

                Entity entity = new Entity();
                entity.address = tempEntityAddress; // if valid store it in entity address

                UpdateEntity(entity); // get health and origin for the player temp address 

                if (entity.Health < 1 || entity.Health > 100 || entity.origin.X > 10000000) // check health if out of range to skip it (probably not a player if health offset it out of range)
                    continue; 

                if (!entities.Any(element => element.origin.X == entity.origin.X)) // remove all dublicate players entities
                {
                    entities.Add(entity);
                    if (entity.teamNum == localplayer.teamNum) // check which team is the entity, and if it the same as player it will be added to player team list
                    {
                        playerTeam.Add(entity);
                    } else
                    {
                        enemyTeam.Add(entity);
                    }
                }
            }
        }
        void UpdateEntity(Entity entity) // read and store health and origin entities
        {

            // 3d
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.viewOffset = new Vector3(0, 0, 65);
            entity.abs = Vector3.Add(entity.origin, entity.viewOffset);

            // 2d, have to calculate 2d before 3d
            var currentViewMatrix = ReadviewMatrix(client + offsets.viewmatrix);
            entity.ScreenPosition = Vector2.Add(WorldToScreen(currentViewMatrix, entity.origin, (int)windowSize.X, (int)windowSize.Y), windowLocation);
            entity.absScreenPosition = Vector2.Add(WorldToScreen(currentViewMatrix, entity.abs, (int)windowSize.X, (int)windowSize.Y), windowLocation);

            // 1d
            entity.angleDifference = CalculatePixelDistance(windowCenter, entity.absScreenPosition); // for aimbot (closer to crosshair)
            entity.Health = swed.ReadInt(entity.address, offsets.health);
            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.teamNum = swed.ReadInt(entity.address, offsets.team);
            entity.jumpFlag = swed.ReadInt(entity.address, offsets.jumpflag);
            entity.magnitude = CalculateMagnitude(localplayer.origin, entity.origin); // for aimbot (closer in distance)

        }


        static void Main(string[] args)
         {
            Program program = new Program();
            program.Start().Wait();

            Thread mainlogicThread = new Thread(program.MainLogic) { IsBackground = true };
            mainlogicThread.Start(); // Begin a Thread to MainLogic
        }
    }
}
