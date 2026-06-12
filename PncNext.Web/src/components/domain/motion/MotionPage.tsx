import { useState } from 'react';
import styled from 'styled-components';
import AxisMonitor from './AxisMonitor';
import MachineStatePanel from './MachineStatePanel';
import JogController from './JogController';
import CommandConsole from './CommandConsole';
import IOStatusDetailed from './IOStatusDetailed';

const PageContainer = styled.div`
  display: flex;
  flex-direction: column;
  gap: 20px;
`;

const TabNav = styled.div`
  display: flex;
  gap: 8px;
  border-bottom: 2px solid var(--border-color);
  margin-bottom: 20px;
`;

const TabButton = styled.button<{ active?: boolean }>`
  padding: 12px 30px;
  background: ${props => props.active ? 'var(--panel-bg)' : '#f1f3f5'};
  color: ${props => props.active ? 'var(--accent-color)' : 'var(--text-muted)'};
  border-radius: 8px 8px 0 0;
  font-weight: 800;
  font-size: 14px;
  border: 1px solid var(--border-color);
  border-bottom: ${props => props.active ? 'none' : '1px solid var(--border-color)'};
  margin-bottom: -2px;
  transition: all 0.2s ease;
  
  &:hover { background: ${props => props.active ? 'var(--panel-bg)' : '#e9ecef'}; }
`;

const DashboardGrid = styled.div`
  display: grid;
  grid-template-columns: 1.2fr 1fr;
  gap: 20px;
`;

const MotionPage = () => {
  const [activeTab, setActiveTab] = useState<'motion' | 'io'>('motion');

  // Mock data (to be replaced with SignalR/API later)
  const mockPositions = [
    { axis: 'X', g53: 120.450, g54: 100.450 },
    { axis: 'Y', g53: -45.220, g54: -65.220 },
    { axis: 'Z', g53: 12.000, g54: 2.000 },
    { axis: 'A', g53: 0.000, g54: 0.000 },
    { axis: 'B', g53: 0.000, g54: 0.000 },
  ];

  const mockState = {
    gplErrorCode: '0x0000',
    homeStatus: 'COMPLETE',
    toolNo: '5',
    toolLength: '120.45',
    spindleRun: true,
    spindleRpm: 12000,
    feedrate: 3500,
    overrideS: 100,
    overrideF: 80,
    flags: {
      vogue: true,
      ready: true,
      busy: false,
      pause: false,
      alarm: false,
      spindleOk: true,
      airOk: true,
    }
  };

  const mockIo = {
    inputs: Array(64).fill(false).map((_, i) => i % 12 === 0),
    outputs: Array(64).fill(false).map((_, i) => i === 1 || i === 5 || i === 32)
  };

  return (
    <PageContainer>
      <TabNav>
        <TabButton active={activeTab === 'motion'} onClick={() => setActiveTab('motion')}>
          Motion Monitor
        </TabButton>
        <TabButton active={activeTab === 'io'} onClick={() => setActiveTab('io')}>
          Detailed I/O (64-bit)
        </TabButton>
      </TabNav>

      {activeTab === 'motion' ? (
        <>
          <DashboardGrid>
            <AxisMonitor positions={mockPositions} />
            <MachineStatePanel state={mockState} />
          </DashboardGrid>
          <DashboardGrid>
            <JogController />
            <CommandConsole />
          </DashboardGrid>
        </>
      ) : (
        <IOStatusDetailed inputs={mockIo.inputs} outputs={mockIo.outputs} />
      )}
    </PageContainer>
  );
};

export default MotionPage;
