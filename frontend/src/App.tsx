import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Applications from './pages/Applications';
import NewApplication from './pages/NewApplication';
import ApplicationDetails from './pages/ApplicationDetails';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="applications" element={<Applications />} />
          <Route path="new" element={<NewApplication />} />
          <Route path="applications/:id" element={<ApplicationDetails />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
