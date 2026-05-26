import { RouterProvider } from "react-router";
import { router } from "./routes";
import { AppProvider } from "./context/AppContext";
import ChatWidget from "./components/ChatWidget/ChatWidget";

function App() {
  return (
    <AppProvider>
      <RouterProvider router={router} />
      <ChatWidget />
    </AppProvider>
  );
}

export default App;
