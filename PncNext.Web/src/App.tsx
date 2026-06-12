import { useState } from 'react';
import styled from 'styled-components';
import GlobalStyle from './styles/GlobalStyle';
import SideNavigation from './components/layout/SideNavigation';
import MotionPage from './components/domain/motion/MotionPage';

const AppContainer = styled.div`
  display: flex;
  height: 100vh;
  background-color: #ffffff; /* Pure White */
`;

const MainContent = styled.main`
  flex: 1;
  padding: 30px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
`;

const Header = styled.header`
  margin-bottom: 30px;
  border-bottom: 2px solid var(--border-color);
  padding-bottom: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;

  h1 {
    font-size: 1.8rem;
    color: var(--primary-color);
  }
`;

const StatusBadge = styled.span<{ connected: boolean }>`
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
  background-color: ${props => props.connected ? 'var(--success-color)' : 'var(--error-color)'};
  color: white;
  display: flex;
  align-items: center;
  gap: 8px;

  &::before {
    content: '';
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background-color: white;
    box-shadow: 0 0 5px white;
  }
`;

function App() {
  const [activeMenu, setActiveMenu] = useState('motion');

  const renderContent = () => {
    switch (activeMenu) {
      case 'motion':
        return <MotionPage />;
      case 'disk':
        return (
          <section>
            <h2>Disk Inventory Management</h2>
            <p>Manage dental pucks/blocks and visualize used area layouts.</p>
          </section>
        );
      case 'ncfile':
        return (
          <section>
            <h2>NC File Queue</h2>
            <p>Monitor incoming files and manage job preparation.</p>
          </section>
        );
      case 'history':
        return (
          <section>
            <h2>Job History Logs</h2>
            <p>Review past milling results and error reports.</p>
          </section>
        );
      case 'settings':
        return (
          <section>
            <h2>System Settings</h2>
            <p>Configure backend API and SignalR endpoints.</p>
          </section>
        );
      default:
        return <div>Select a menu</div>;
    }
  };

  const getHeaderTitle = () => {
    const titles: Record<string, string> = {
      motion: 'Motion Control',
      disk: 'Disk Management',
      ncfile: 'NC File Queue',
      history: 'Job History',
      settings: 'Settings'
    };
    return titles[activeMenu] || 'Dashboard';
  };

  return (
    <>
      <GlobalStyle />
      <AppContainer>
        <SideNavigation 
          activeMenu={activeMenu} 
          onMenuChange={setActiveMenu} 
        />
        <MainContent>
          <Header>
            <h1>{getHeaderTitle()}</h1>
            <StatusBadge connected={true}>System Online</StatusBadge>
          </Header>
          {renderContent()}
        </MainContent>
      </AppContainer>
    </>
  );
}

export default App;
