XCOPY "..\Client\bin\Debug\net5.0\Oqtane.ChatHubs.Client.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Client\bin\Debug\net5.0\Oqtane.ChatHubs.Client.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Server\bin\Debug\net5.0\Oqtane.ChatHubs.Server.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Server\bin\Debug\net5.0\Oqtane.ChatHubs.Server.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Shared\bin\Debug\net5.0\Oqtane.ChatHubs.Shared.Oqtane.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\Shared\bin\Debug\net5.0\Oqtane.ChatHubs.Shared.Oqtane.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorAlerts\bin\Debug\net5.0\BlazorAlerts.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorAlerts\bin\Debug\net5.0\BlazorAlerts.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorAccordion\bin\Debug\net5.0\BlazorAccordion.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorAccordion\bin\Debug\net5.0\BlazorAccordion.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorSelect\bin\Debug\net5.0\BlazorSelect.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorSelect\bin\Debug\net5.0\BlazorSelect.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorModal\bin\Debug\net5.0\BlazorModal.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorModal\bin\Debug\net5.0\BlazorModal.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorWindows\bin\Debug\net5.0\BlazorWindows.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorWindows\bin\Debug\net5.0\BlazorWindows.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorTabs\bin\Debug\net5.0\BlazorTabs.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorTabs\bin\Debug\net5.0\BlazorTabs.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorPager\bin\Debug\net5.0\BlazorPager.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorPager\bin\Debug\net5.0\BlazorPager.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorDraggableList\bin\Debug\net5.0\BlazorDraggableList.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorDraggableList\bin\Debug\net5.0\BlazorDraggableList.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorFileUpload\bin\Debug\net5.0\BlazorFileUpload.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorFileUpload\bin\Debug\net5.0\BlazorFileUpload.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorColorPicker\bin\Debug\net5.0\BlazorColorPicker.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorColorPicker\bin\Debug\net5.0\BlazorColorPicker.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorVideo\bin\Debug\net5.0\BlazorVideo.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorVideo\bin\Debug\net5.0\BlazorVideo.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorBrowserResize\bin\Debug\net5.0\BlazorBrowserResize.dll" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y
XCOPY "..\BlazorBrowserResize\bin\Debug\net5.0\BlazorBrowserResize.pdb" "..\..\oqtane.framework\Oqtane.Server\bin\Debug\net5.0\" /Y

XCOPY "..\Server\wwwroot\Modules\Oqtane.ChatHubs\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\Oqtane.ChatHubs\" /Y /S /I

XCOPY "..\BlazorAlerts\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\Modules\Oqtane.ChatHubs\" /Y /S /I
XCOPY "..\BlazorVideo\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\BlazorVideo\" /Y /S /I
XCOPY "..\BlazorBrowserResize\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\BlazorBrowserResize\" /Y /S /I
XCOPY "..\BlazorFileUpload\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\BlazorFileUpload\" /Y /S /I
XCOPY "..\BlazorDraggableList\wwwroot\*" "..\..\oqtane.framework\Oqtane.Server\wwwroot\_content\BlazorDraggableList\" /Y /S /I