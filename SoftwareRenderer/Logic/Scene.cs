using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftwareRenderer.Rendering;
using SoftwareRenderer.Helpers;
using System.Windows.Media;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Media.Imaging;

namespace SoftwareRenderer.Logic
{
    public class Scene : IUpdateable
    {
        private readonly Renderer _renderer;

        private ObjData _boardboxObj;
        private ObjData _fieldObj;
        private ObjData _pawnObj;

        private Camera _activeCamera;
        private Camera _secondaryCamera;

        private double _angle;

        private List<Material> _materials;

        public Scene(Renderer renderer)
        {
            _renderer = renderer;

            _fieldObj = ObjData.LoadFromFile("Data/Models/cube.obj");
            _boardboxObj = ObjData.LoadFromFile("Data/Models/boardbox.obj");
            _pawnObj = ObjData.LoadFromFile("Data/Models/pawn.obj");

            _activeCamera = new Camera();
            _activeCamera.NearPlane = 0.1;
            _activeCamera.FarPlane = 200.0;
            _activeCamera.Position = VectorHelpers.Create(0, 10, -20);
            _activeCamera.LookAt = VectorHelpers.Create(0, 0, 0);
            _activeCamera.UpVector = VectorHelpers.Create(0, 1, 0);

            _secondaryCamera = new Camera();
            _secondaryCamera.NearPlane = 0.1;
            _secondaryCamera.FarPlane = 200.0;
            _secondaryCamera.Position = VectorHelpers.Create(0, 5, -20);
            _secondaryCamera.LookAt = VectorHelpers.Create(0, 0, 0);
            _secondaryCamera.UpVector = VectorHelpers.Create(0, 1, 0);

            _renderer.Lights.Add(new PointLight
            {
                Position = VectorHelpers.Create(0, 10, -20),
                Color = Colors.White,
            });

            //_renderer.Lights.Add(new PointLight
            //{
            //    Position = VectorHelpers.Create(-10, 1.5, 0),
            //    Color = Colors.Blue,
            //});

            //_renderer.Lights.Add(new PointLight
            //{
            //    Position = VectorHelpers.Create(0, 10, 0),
            //    Color = Colors.White,
            //});

            _materials = new List<Material>();

            _materials.Add(new Material
            {
                AmbientColor = Colors.Black,
                DiffuseColor = Colors.Chartreuse,
                SpecularColor = Colors.Gray,
                DiffuseTexture = new WriteableBitmap(new BitmapImage(new Uri("Data/Textures/darkstone.png", UriKind.Relative))),
                ShineFactor = 128.0,
            });

            _materials.Add(new Material
            {
                AmbientColor = Colors.Black,
                DiffuseColor = Colors.White,
                SpecularColor = Colors.Gray,
                ShineFactor = 1,
            });

            _materials.Add(new Material
            {
                AmbientColor = Color.FromRgb(16,16,16),
                DiffuseColor = Colors.RosyBrown,
                SpecularColor = Colors.White,
                DiffuseTexture = new WriteableBitmap(new BitmapImage(new Uri("Data/Textures/wood.png", UriKind.Relative))),
                ShineFactor = 1.0,
            });

            _materials.Add(new Material
            {
                AmbientColor = Colors.Black,
                DiffuseColor = Colors.DarkRed,
                SpecularColor = Colors.Yellow,
                ShineFactor = 64.0,
            });

        }

        public void MoveCamera(double forward, double side)
        {
            var cameraForward = (_activeCamera.LookAt - _activeCamera.Position).Normalize(2);
            var cameraSide = VectorHelpers.CrossProduct3D(cameraForward, _activeCamera.UpVector);

            _activeCamera.Position = _activeCamera.Position + cameraForward * forward + cameraSide * side;
        }

        double _pawnX = 0;
        double _pawnZ = 0;

        public void MovePawn(double px, double pz)
        {
            _pawnX += px;
            _pawnZ += pz;
        }

        public void SwapCameras()
        {
            Camera temp = _activeCamera;
            _activeCamera = _secondaryCamera;
            _secondaryCamera = temp;
        }

        public void Update(TimeSpan elapsedTime)
        {
            _angle += elapsedTime.TotalSeconds * 0.25;
        }

        public void Render()
        {
            var viewMatrix = _activeCamera.GetViewMatrix();
            var projMatrix = _activeCamera.GetProjectionMatrix();
            var chessboardMatrix = MatrixHelpers.RotationY(_angle);

            var projView = projMatrix * viewMatrix;
            var projViewModel = projMatrix * viewMatrix * chessboardMatrix;

            var pawnModelMatrix = chessboardMatrix * MatrixHelpers.Translation(_pawnX, 1, _pawnZ);
            var pawnFinalMatrix = projView * pawnModelMatrix;

            _renderer.WorldPositionEye = _activeCamera.Position;

            _renderer.BeginFrame();

            _renderer.Material = _materials[2];
            _renderer.RenderObjMesh(_boardboxObj, projViewModel, chessboardMatrix);

            for (int z = 0; z < 8; ++z)
            {
                for (int x = 0; x < 8; ++x)
                {
                    int materialId = (z + x % 2) % 2;
                    _renderer.Material = _materials[materialId];

                    var modelMatrix = MatrixHelpers.Translation(2*(x-4)+1, 0, 2*(z-4)+1);
                    var finalMatrix = projView * chessboardMatrix * modelMatrix;
                    _renderer.RenderObjMesh(_fieldObj, finalMatrix, chessboardMatrix);
                }
            }

            _renderer.Material = _materials[3];
            _renderer.RenderObjMesh(_pawnObj, pawnFinalMatrix, pawnModelMatrix);

            _renderer.EndFrame();
        }
    }
}
