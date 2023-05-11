using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Prism.Commands;

using TEMA_3_DZ_7_1_Library;

namespace TEMA_3_DZ_7_1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        private Document _doc;

        public List<FamilySymbol> TitleBlockList { get; } = new List<FamilySymbol>();           
        public List<FamilyInstance> TitleBlockInstanceList { get; set; } = new List<FamilyInstance>();
        public List<View> ViewList { get; } = new List<View>();
        public DelegateCommand CreateCommand { get; }
        public int ListCounts { get; set; }
        public FamilySymbol SelectedTitleBlock { get; set; }
        public View SelectedView { get; set; }
        public string Designer { get; set; }


        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _doc = _commandData.Application.ActiveUIDocument.Document;

            TitleBlockList = TitleBlockUtils.GetTitleBlocks(_commandData);
            TitleBlockInstanceList = TitleBlockInstanceUtils.GetTitleBlocksInstance(_commandData);
            ViewList = ViewUtils.GetViews(_commandData);
            CreateCommand = new DelegateCommand(OnCreateCommand);
        }

        private void OnCreateCommand()
        {
            Document doc = _commandData.Application.ActiveUIDocument.Document;

            if (TitleBlockList == null || SelectedTitleBlock == null || ListCounts < 1)
            {
                return;
            }

            using (var ts = new Transaction(doc, "Create sheet"))
            {

                ts.Start();

                List<ElementId> sheetIdList = new List<ElementId>();
                List<ViewSheet> sheetList = new List<ViewSheet>();

                XYZ point;
                point = new XYZ(1.1, 0.9, 0); // точка для вставки вида

                for (int i = 0; i < ListCounts; i++)
                {
                    //создаем листы
                    sheetList.Add(ViewSheet.Create(doc, SelectedTitleBlock.Id));
                }
                foreach (var sheet in sheetList)
                {
                    // соберем Id всех листов
                    sheetIdList.Add(sheet.Id);
                }
                //создаем виды на листах
                for (int i = 0; i < ListCounts; i++)
                {
                    Viewport.Create(doc, sheetIdList[i], SelectedView.Duplicate(ViewDuplicateOption.WithDetailing), point);
                }
                //указываем разработчика
                foreach (var sheet in sheetList)
                {
                    Parameter designByParameter = sheet.LookupParameter("Designed By");
                    designByParameter.Set(Designer);
                }

                ts.Commit();

            }
            RaiseCloseRequest();

        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
