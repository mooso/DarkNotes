import si.SOMEEngine.EngineManager;
import si.SOMEEngine.SOMEEngine;
import si.SOMEEngine.EngineManagerIfc.EngineType;

import si.SOMEInterface.SOMEContext;
import si.SOMEInterface.components.ClientInteractor;

private static EngineManager engineManager;
	private static ClientInteractor clientInteractor;

	private void initEnv(String titleName, String propFileName) {
		String initMsg = SOMEContext.initServer(false, propFileName); // false ==> no client server setting
		if (initMsg == null) {
			initMsg = Initializer.load(Environment.APPL_IDE, titleName);
		}

		if (initMsg != null) {
			GUIUtils.showError("Initialization problem: \n" + initMsg);
			System.exit(1);
		}

		// initialize ClientInteractor, based on SOME context
		clientInteractor = ClientInteractor.getClientInteractor();
		try {
			clientInteractor.initialize(cNAMED_IDE_SOME_CONTEXT, null, false, true, false, true);
		} catch (Exception e) {
			e.printStackTrace();
		}

		engineManager.getEngine(EngineType.ET_SINGLE_TEXT, titleName);
	}
